using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MezzexEye.Controllers
{
    public class OfficeLeaveController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfficeLeaveController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string leaveType, string searchUser, DateTime? searchDate, string countryName)
        {
            var loggedInUser = await _userManager.GetUserAsync(User); // Get the logged-in user
            var userRoles = await _userManager.GetRolesAsync(loggedInUser); // Get user roles

            var leaves = _context.OfficeLeaves.Include(o => o.User).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(countryName))
                leaves = leaves.Where(o => o.CountryName == countryName);

            if (!string.IsNullOrEmpty(leaveType))
                leaves = leaves.Where(o => o.LeaveType == leaveType);

            if (searchDate.HasValue)
            {
                leaves = leaves.Where(o =>
                    (o.StartDate.HasValue && o.StartDate.Value.Date <= searchDate.Value.Date) &&
                    (o.EndDate.HasValue && o.EndDate.Value.Date >= searchDate.Value.Date));
            }

            if (!string.IsNullOrEmpty(searchUser))
                leaves = leaves.Where(o => o.User.UserName.Contains(searchUser));

            // Restrict view to logged-in user's leaves if not admin
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Administrator"))
                leaves = leaves.Where(o => o.UserId == loggedInUser.Id);

            // Pass user roles to the view using ViewData
            ViewData["UserRoles"] = userRoles;

            // Pass filter values to the view
            ViewData["CountryName"] = countryName;
            ViewData["LeaveType"] = leaveType;
            ViewData["SearchDate"] = searchDate?.ToString("yyyy-MM-dd"); // Format date for input field
            ViewData["SearchUser"] = searchUser;

            return View(await leaves.ToListAsync());
        }

        // GET: OfficeLeave/Create
        public async Task<IActionResult> Create()
        {
            var loggedInUser = await _userManager.GetUserAsync(User); // Get the logged-in user
            var isAdmin = await _userManager.IsInRoleAsync(loggedInUser, "Administrator"); // Check if the user is an admin

            // Fetch users: All for admins, only the logged-in user for normal users
            var users = isAdmin
                ? _userManager.Users.ToList()
                : _userManager.Users.Where(u => u.Id == loggedInUser.Id).ToList();

            // Pass users to the view
            ViewBag.Users = users.Select(u => new { u.Id, Name = $"{u.FirstName} {u.LastName}" });

            return View();
        }

        // POST: OfficeLeave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfficeLeave officeLeave)
        {
            var loggedInUser = await _userManager.GetUserAsync(User); // Get the logged-in user
            // Validate time fields for India
            if (officeLeave.CountryName == "India" && (officeLeave.StartTime.HasValue || officeLeave.EndTime.HasValue))
            {
                ModelState.AddModelError("StartTime", "Time is not allowed for India.");
            }

            // Validate the leave model and save it
            if (ModelState.IsValid)
            {
                officeLeave.Status = "Pending";
                officeLeave.CountryName = loggedInUser.CountryName;
                _context.Add(officeLeave);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload users for the dropdown in case of validation failure
            var isAdmin = await _userManager.IsInRoleAsync(loggedInUser, "Administrator");

            var users = isAdmin
                ? _userManager.Users.ToList()
                : _userManager.Users.Where(u => u.Id == loggedInUser.Id).ToList();

            ViewBag.Users = users.Select(u => new { u.Id, Name = $"{u.FirstName} {u.LastName}" });

            return View(officeLeave);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var leave = await _context.OfficeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            leave.Status = "Approved";
            _context.Update(leave);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var leave = await _context.OfficeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            leave.Status = "Rejected";
            _context.Update(leave);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var leave = await _context.OfficeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }
            return View(leave);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OfficeLeave updatedLeave)
        {
            if (id != updatedLeave.LeaveId)
            {
                return BadRequest();
            }

            var leave = await _context.OfficeLeaves.FindAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            // Update fields
            leave.LeaveType = updatedLeave.LeaveType;
            leave.StartDate = updatedLeave.StartDate;
            leave.EndDate = updatedLeave.EndDate;
            leave.CountryName = updatedLeave.CountryName;

            try
            {
                _context.Update(leave);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.OfficeLeaves.Any(e => e.LeaveId == id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var leave = await _context.OfficeLeaves.FindAsync(id);
            if (leave != null)
            {
                _context.OfficeLeaves.Remove(leave);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
