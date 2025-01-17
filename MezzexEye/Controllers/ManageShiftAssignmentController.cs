using System;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using MezzexEye.Services;
using MezzexEye.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MezzexEye.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using EyeMezzexz.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MezzexEye.Controllers
{
    public class ManageShiftAssignmentController : Controller
    {
        private readonly IShiftAssignmentApiService _shiftAssignmentService;
        private readonly IShiftApiService _shiftService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ManageShiftAssignmentController> _logger;
        private readonly ApplicationDbContext _context;

        public ManageShiftAssignmentController(
            IShiftAssignmentApiService shiftAssignmentService,
            IShiftApiService shiftService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<ManageShiftAssignmentController> logger)
        {
            _shiftAssignmentService = shiftAssignmentService;
            _shiftService = shiftService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? selectedWeekNumber, string selectedCountry = "India", int? searchShiftId = null, string searchUsername = null)
        {
            int currentWeekNumber = ISOWeek.GetWeekOfYear(DateTime.Now);
            int weekNumber = selectedWeekNumber ?? currentWeekNumber;

            DateTime firstDayOfYear = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime startOfWeek = firstDayOfYear.AddDays((weekNumber - 1) * 7).StartOfWeek(DayOfWeek.Sunday);
            DateTime weekEnd = startOfWeek.AddDays(6);

            var country = await _context.Countries.FirstOrDefaultAsync(c => c.Name == selectedCountry);
            if (country == null)
            {
                _logger.LogWarning($"Country '{selectedCountry}' not found.");
                ModelState.AddModelError("", "Selected country not found.");
                return View(new ShiftAssignmentViewModel());
            }

            // Filter users by country and optionally by username
            var users = _userManager.Users.Where(u => u.CountryName == selectedCountry);
            if (!string.IsNullOrEmpty(searchUsername))
            {
                users = users.Where(u => $"{u.FirstName} {u.LastName}".Contains(searchUsername, StringComparison.OrdinalIgnoreCase));
            }

            var userList = await users.ToListAsync();

            // Retrieve shifts and filter assignments by week
            var shifts = await _shiftService.GetShiftsByCountryAsync(country.Id);
            var existingAssignments = await _shiftAssignmentService.GetShiftAssignmentsAsync(startOfWeek, weekEnd, selectedCountry);

            // Filter users based on selected shift ID
            if (searchShiftId.HasValue)
            {
                var usersWithShift = existingAssignments
                    .Where(a => a.ShiftId == searchShiftId.Value)
                    .Select(a => a.UserId)
                    .Distinct()
                    .ToList();

                userList = userList.Where(u => usersWithShift.Contains(u.Id)).ToList();
            }

            var dayWiseAvailableHours = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Select(day => new DayAvailableHours
                {
                    Day = day,
                    AvailableHours = existingAssignments
                        .Where(a => a.Day == day)
                        .Sum(a => (a.Shift.ToTime - a.Shift.FromTime).TotalHours)
                })
                .ToList();

            var model = new ShiftAssignmentViewModel
            {
                AssignedDate = startOfWeek,
                AssignedWeekDisplay = $"{startOfWeek:MMM dd} - {weekEnd:MMM dd}",
                SelectedWeekNumber = weekNumber,
                SelectedCountry = selectedCountry,
                Shifts = shifts,
                CountryList = new List<SelectListItem>
        {
            new SelectListItem { Text = "India", Value = "India" },
            new SelectListItem { Text = "United Kingdom", Value = "United Kingdom" }
        },
                SearchShiftId = searchShiftId,  // Pass the selected shift ID to the view
                UserShiftAssignments = userList.Select(user => new UserShiftAssignment
                {
                    UserId = user.Id,
                    UserName = $"{user.FirstName} {user.LastName}",
                    DayWiseAssignments = Enum.GetValues(typeof(DayOfWeek))
                        .Cast<DayOfWeek>()
                        .Select(day => new DayShiftAssignment
                        {
                            Day = day,
                            ShiftIds = existingAssignments
                                .Where(a => a.UserId == user.Id && a.Day == day)
                                .Select(a => a.ShiftId)
                                .ToList()
                        })
                        .ToList()
                }).ToList(),
                DayWiseAvailableHours = dayWiseAvailableHours
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveShiftAssignment(ShiftAssignmentViewModel model)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Reload shifts and return the view if the model is invalid
                model.Shifts = await _shiftService.GetShiftsAsync();
                return View("Index", model);
            }

            try
            {
                DateTime weekStart = model.AssignedDate;
                DateTime weekEnd = weekStart.AddDays(6);

                foreach (var userAssignment in model.UserShiftAssignments)
                {
                    // Remove existing assignments for the user within the specified week to avoid duplication
                    await _shiftAssignmentService.RemoveAssignmentsForUserInWeekAsync(userAssignment.UserId, weekStart, weekEnd);

                    // Iterate through each day's shift assignments
                    foreach (var assignment in userAssignment.DayWiseAssignments)
                    {
                        var shiftIds = assignment.ShiftIds?.ToList();
                        if (shiftIds != null && shiftIds.Count > 0)
                        {
                            // Retrieve and order shifts by start time
                            var shiftIdsString = string.Join(",", shiftIds);
                            string sqlQuery = $"SELECT * FROM Shifts WHERE ShiftId IN ({shiftIdsString}) ORDER BY FromTime;";

                            var selectedShifts = await _context.Shifts
                                .FromSqlRaw(sqlQuery)
                                .ToListAsync();

                            // Validate that each shift's end time is before the start time of the next shift
                            bool isValidAssignment = ValidateShiftSequence(selectedShifts);

                            if (isValidAssignment)
                            {
                                // Create and add each valid shift assignment independently for the day
                                foreach (var shift in selectedShifts)
                                {
                                    var shiftAssignment = new ShiftAssignment
                                    {
                                        UserId = userAssignment.UserId,
                                        ShiftId = shift.ShiftId,
                                        Day = assignment.Day,
                                        AssignedOn = model.AssignedDate.AddDays((int)assignment.Day - (int)model.AssignedDate.DayOfWeek),
                                        CreatedOn = DateTime.Now,
                                        CreatedBy = User.Identity?.Name // Use null-conditional operator for safety
                                    };

                                    // Save each shift assignment separately
                                    await _shiftAssignmentService.AddShiftAssignmentAsync(shiftAssignment);
                                }
                            }
                            else
                            {
                                // Add an error message for invalid shift sequences
                                ModelState.AddModelError("", $"Invalid shift sequence for user {userAssignment.UserName} on {assignment.Day}");
                                return RedirectToAction("Index", model);
                            }
                        }
                    }
                }

                // Redirect to the Index action upon successful save
                return RedirectToAction("Index", new { selectedWeekNumber = model.SelectedWeekNumber, selectedCountry = model.SelectedCountry });
            }
            catch (Exception ex)
            {
                // Log the exception and add a generic error message to ModelState
                _logger.LogError(ex, "Error saving shift assignments.");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the shift assignments.");

                // Reload shifts to ensure the view model has necessary data
                model.Shifts = await _shiftService.GetShiftsAsync();
                return View("Index", model);
            }
        }

        private bool ValidateShiftSequence(List<Shift> shifts)
        {
            // Ensure that each shift in the list follows a proper sequence
            for (int i = 0; i < shifts.Count - 1; i++)
            {
                if (shifts[i].ToTime > shifts[i + 1].FromTime)
                {
                    // If the end time of the current shift is after the start time of the next, return false
                    return false;
                }
            }
            return true;
        }


    }
}
