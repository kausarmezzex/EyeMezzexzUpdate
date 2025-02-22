using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Data;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficeLeaveController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OfficeLeaveController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("getLeavesByUser")]
        public async Task<IActionResult> GetLeavesByUser(int userId)
        {
            var leaves = await _context.OfficeLeave
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedOn) // ✅ Sorting by latest leave created
                .ToListAsync();

            if (leaves == null || !leaves.Any())
            {
                return NotFound("No leaves found for this user.");
            }

            var userAccount = await _context.UserAccountDetail
                .Include(u => u.ApplicationUser) // ✅ Ensure ApplicationUser is loaded
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (userAccount == null)
            {
                return NotFound("User account details not found.");
            }

            string countryName = leaves.FirstOrDefault()?.CountryName ?? "Unknown";

            // ✅ Convert data into List<OfficeLeaveResponse>
            var response = leaves.Select(leave => new OfficeLeaveResponse
            {
                Leave = leave,
                UserDetails = new UserDetailsResponse
                {
                    UserId = userAccount.UserId,
                    FirstName = userAccount.ApplicationUser?.FirstName ?? "N/A",
                    RemainingHolidayDays = userAccount.RemainingYearlyLeave,
                    RemainingHolidayHours = countryName == "United Kingdom" ? userAccount.RemainingYearlyHours : (int?)null
                }
            }).ToList();

            return Ok(response);
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateLeave([FromBody] OfficeLeave leave)
        {
            if (leave == null)
            {
                return BadRequest("Invalid leave request.");
            }

            // Step 1: Retrieve the logged-in user's details using the UserId
            var user = await _context.Users
                                     .Where(u => u.Id == leave.UserId && u.Active == true)
                                     .Select(u => new { u.FirstName, u.LastName })
                                     .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found or inactive." });
            }

            // Step 2: Set the CreatedBy field to the user's first name and last name
            leave.CreatedBy = $"{user.FirstName} {user.LastName}";
            leave.CreatedOn = DateTime.UtcNow; // Automatically setting created date
            leave.Status = "Pending";

            // Step 3: Add the leave request to the context and save changes
            _context.OfficeLeave.Add(leave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave created successfully", leave });
        }

        // **2. Edit Leave (PUT)**
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditLeave(int id, [FromBody] OfficeLeave updatedLeave)
        {
            // Step 1: Retrieve the logged-in user's details using the UserId
            var user = await _context.Users
                                     .Where(u => u.Id == updatedLeave.UserId && u.Active == true)
                                     .Select(u => new { u.FirstName, u.LastName })
                                     .FirstOrDefaultAsync();
            var leave = await _context.OfficeLeave.FindAsync(id);
            if (leave == null)
            {
                return NotFound("Leave not found.");
            }

            // Updating leave properties
            leave.LeaveType = updatedLeave.LeaveType;
            leave.StartDate = updatedLeave.StartDate;
            leave.EndDate = updatedLeave.EndDate;
            leave.StartTime = updatedLeave.StartTime;
            leave.EndTime = updatedLeave.EndTime;
            leave.Status = updatedLeave.Status;
            leave.Notes = updatedLeave.Notes;
            leave.LeavePaymentType = updatedLeave.LeavePaymentType;
            leave.PaidLeaveDays = updatedLeave.PaidLeaveDays;
            leave.PaidLeaveHours = updatedLeave.PaidLeaveHours;
            leave.StatusChangeBy = updatedLeave.StatusChangeBy;
            leave.StatusChangeOn = updatedLeave.StatusChangeOn;
            leave.AdminComment = updatedLeave.AdminComment;
            leave.ModifyOn = DateTime.Now;
            leave.ModifyBy = user.FirstName + " " + user.LastName;
            _context.OfficeLeave.Update(leave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave updated successfully", leave });
        }

        // **3. Delete Leave (DELETE)**
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leave = await _context.OfficeLeave.FindAsync(id);
            if (leave == null)
            {
                return NotFound("Leave not found.");
            }

            _context.OfficeLeave.Remove(leave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave deleted successfully" });
        }

        // **4. Get All Leaves (GET)**
        [HttpGet("all")]
        public async Task<IActionResult> GetAllLeaves()
        {
            var leaves = await _context.OfficeLeave.ToListAsync();
            return Ok(leaves);
        }
        // **5. Get Leave By Id (GET)**
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetLeaveById(int id)
        {
            var leave = await _context.OfficeLeave.FindAsync(id);
            if (leave == null)
            {
                return NotFound("Leave not found.");
            }
             
            return Ok(leave);
        }
        public class OfficeLeaveResponse
        {
            public OfficeLeave Leave { get; set; }
            public UserDetailsResponse UserDetails { get; set; }
        }

        public class UserDetailsResponse
        {
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public int? RemainingHolidayDays { get; set; }
            public int? RemainingHolidayHours { get; set; }
        }

    }
}
