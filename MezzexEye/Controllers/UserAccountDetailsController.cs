using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MezzexEye.Controllers
{
    public class UserAccountDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserAccountDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserAccountDetails
        public IActionResult Index()
        {
            // Include ApplicationUser to fetch user details
            var userAccounts = _context.UserAccountDetails
                .Include(u => u.ApplicationUser) // Include navigation property
                .ToList();

            return View(userAccounts);
        }

        [HttpGet("UserAccountDetails/Create/{userId}")] // Add route template
        public IActionResult Create(int? userId)
        {
            ViewBag.Users = _context.Users
     .Select(u => new
     {
         Id = u.Id,
         FullName = $"{u.FirstName} {u.LastName}"
     })
     .ToList()
     .Select(u => new SelectListItem
     {
         Value = u.Id.ToString(),
         Text = u.FullName
     });
            ViewBag.Companies = new List<SelectListItem>
    {
        new SelectListItem { Text = "Direct Care LTD.", Value = "Direct Care LTD." },
        new SelectListItem { Text = "Aster Care LTD.", Value = "Aster Care LTD." },
        new SelectListItem { Text = "Zippy Care", Value = "Zippy Care" },
        new SelectListItem { Text = "Ultior LTD", Value = "Ultior LTD" },
        new SelectListItem { Text = "Jk Consultancy", Value = "Jk Consultancy" }
    };
            ViewBag.AgreementTypes = new List<SelectListItem>
    {
        new SelectListItem { Value = "Cash", Text = "Cash" },
        new SelectListItem { Value = "Payroll", Text = "Payroll" },
        new SelectListItem { Value = "Cash+Payroll", Text = "Cash+Payroll" }
    };
            if (userId == null)
            {
                return NotFound();
            }

            // Fetch user details from ApplicationUser
            var user = _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber,
                    u.CountryName
                })
                .FirstOrDefault();

            if (user == null)
            {
                return NotFound();
            }

            // Prepopulate the form with user data
            var userAccountDetails = new UserAccountDetails
            {
                UserId = user.Id, // Set the foreign key
            };

            // Pass the model to the view
            ViewBag.UserDetails = user; // Pass user details for display
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserAccountDetails userAccountDetails)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userAccountDetails);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown if model validation fails
            ViewBag.Users = _context.Users
    .Select(u => new
    {
        Id = u.Id,
        FullName = $"{u.FirstName} {u.LastName}"
    })
    .ToList()
    .Select(u => new SelectListItem
    {
        Value = u.Id.ToString(),
        Text = u.FullName
    });
            // Fetch user details again if model validation fails
            var user = _context.Users
                .Where(u => u.Id == userAccountDetails.UserId)
                .FirstOrDefault();

            ViewBag.UserDetails = user;
            return View(userAccountDetails);
        }


        // GET: UserAccountDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
 

            // Fetch UserAccountDetails with ApplicationUser
            var userAccount = await _context.UserAccountDetails
                .Include(u => u.ApplicationUser)
                .FirstOrDefaultAsync(u => u.AccountDetailsId == id);

            if (userAccount == null)
            {
                return NotFound();
            }

            // Pass user details for display in the view
            ViewBag.UserDetails = new
            {
                userAccount.ApplicationUser.FirstName,
                userAccount.ApplicationUser.LastName,
                userAccount.ApplicationUser.Email,
                userAccount.ApplicationUser.PhoneNumber,
                userAccount.ApplicationUser.CountryName
            };

            if (userAccount == null) return NotFound();

            // Populate the dropdown with countries
            ViewBag.Users = _context.Users
    .Select(u => new
    {
        Id = u.Id,
        FullName = $"{u.FirstName} {u.LastName}"
    })
    .ToList()
    .Select(u => new SelectListItem
    {
        Value = u.Id.ToString(),
        Text = u.FullName
    });

            return View(userAccount);
        }

        // POST: UserAccountDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserAccountDetails userAccountDetails)
        {
            if (id != userAccountDetails.AccountDetailsId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(userAccountDetails);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown if model validation fails
            ViewBag.Users = _context.Users
    .Select(u => new
    {
        Id = u.Id,
        FullName = $"{u.FirstName} {u.LastName}"
    })
    .ToList()
    .Select(u => new SelectListItem
    {
        Value = u.Id.ToString(),
        Text = u.FullName
    });
            // Fetch user details again if model validation fails
            var user = _context.Users
                .Where(u => u.Id == userAccountDetails.UserId)
                .FirstOrDefault();

            ViewBag.UserDetails = user;
            return View(userAccountDetails);
        }

        // GET: UserAccountDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userAccount = await _context.UserAccountDetails.FindAsync(id);
            if (userAccount == null) return NotFound();

            return View(userAccount);
        }

        // POST: UserAccountDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userAccount = await _context.UserAccountDetails.FindAsync(id);
            if (userAccount != null)
            {
                _context.UserAccountDetails.Remove(userAccount);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
