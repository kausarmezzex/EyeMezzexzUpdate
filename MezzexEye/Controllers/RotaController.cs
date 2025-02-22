using EyeMezzexz.Data;
using MezzexEye.Helpers;
using MezzexEye.Models;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Authorization;

namespace MezzexEye.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class RotaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RotaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
     int? selectedYear,
     int? selectedMonth,
     int? selectedDay,
     int? selectedWeekNumber, // Add this parameter
     string selectedCountry = "India",
     string searchUsername = null)
        {
            // Step 1: Calculate Week Dates Based on Selected Year, Month, and Day
            DateTime selectedDate = DateTime.UtcNow;
            // If year and month are selected, set the selected date to the first day of the month
            if (selectedYear.HasValue && selectedMonth.HasValue)
            {
                selectedDate = new DateTime(selectedYear.Value, selectedMonth.Value, 1); // Default to the first day of the month
            }

/*            // If day is also selected, update the selected date
            if (selectedDay.HasValue)
            {
                selectedDate = new DateTime(selectedYear.Value, selectedMonth.Value, selectedDay.Value);
            } */  

            int weekNumber = selectedWeekNumber ?? ISOWeek.GetWeekOfYear(selectedDate);
            DateTime startOfWeek = ISOWeek.ToDateTime(selectedDate.Year, weekNumber, DayOfWeek.Monday);
            DateTime endOfWeek = startOfWeek.AddDays(6);
            // Ensure the week falls within the selected month
            if (selectedMonth.HasValue && (startOfWeek.Month != selectedMonth.Value || endOfWeek.Month != selectedMonth.Value))
            {
                // Adjust the start and end of the week to stay within the selected month
                startOfWeek = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                endOfWeek = startOfWeek.AddDays(6);

                // Recalculate the week number
                weekNumber = ISOWeek.GetWeekOfYear(startOfWeek);
            }

            // Step 1: Fetch Users Filtered by Country
            var usersQuery = _context.Users.Where(u => u.CountryName == selectedCountry);

            if (!string.IsNullOrEmpty(searchUsername))
            {
                usersQuery = usersQuery.Where(a =>
                    EF.Functions.Like(a.FirstName, $"%{searchUsername}%") ||
                    EF.Functions.Like(a.LastName, $"%{searchUsername}%"));
            }

            var users = await usersQuery.ToListAsync(); // ToListAsync() ko last me call karein



            // Step 3: Fetch RotaDetails for the week
            var rotaDetails = await _context.StaffRota
                .Where(rd => rd.RotaDate >= startOfWeek && rd.RotaDate <= endOfWeek)
                .ToListAsync();

            // Step 4: Fetch ShiftAssignments for the week
            var shiftAssignments = await _context.ShiftAssigned
                .Include(sa => sa.Shift)
                .Where(sa => sa.AssignedOn >= startOfWeek && sa.AssignedOn <= endOfWeek)
                .ToListAsync();

            // Step 5: Fetch Leaves for the week
            var leaves = await _context.OfficeLeave
                .Where(l => l.Status != "Rejected" &&
                            ((l.StartDate <= endOfWeek && l.EndDate >= startOfWeek)))
                .ToListAsync();

            // Step 6: Calculate Total Available Hours for the Week
            double totalAvailableHoursForWeek = 0;

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startOfWeek.AddDays(i);

                // Calculate hours from RotaDetails (only if there is a corresponding shift assignment)
                var rotaHoursForDay = rotaDetails
                    .Where(rd => rd.RotaDate.Date == currentDate.Date &&
                                 rd.ShiftStartTime != TimeSpan.Zero &&
                                 rd.ShiftEndTime != TimeSpan.Zero &&
                                 shiftAssignments.Any(sa => sa.UserId == rd.UserId && sa.AssignedOn.Date == currentDate.Date))
                    .Sum(rd => (CalculateShiftHours(rd.ShiftStartTime, rd.ShiftEndTime)));

                // Calculate hours from ShiftAssignments (excluding users and dates already in RotaDetails)
                var shiftHoursForDay = shiftAssignments
                    .Where(sa => sa.AssignedOn.Date == currentDate.Date &&
                                 !rotaDetails.Any(rd => rd.UserId == sa.UserId && rd.RotaDate.Date == currentDate.Date))
                    .Sum(sa => CalculateShiftHours(sa.Shift.FromTime, sa.Shift.ToTime));

                totalAvailableHoursForWeek += rotaHoursForDay + shiftHoursForDay;
            }

            // Step 7: Organize Data for ViewModel
            var userRotas = users.Select(user => new UserRota
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                DayWiseRota = Enumerable.Range(0, 7).Select(i =>
                {
                    var currentDate = startOfWeek.AddDays(i);
                    var shiftAssignment = shiftAssignments.FirstOrDefault(sa => sa.UserId == user.Id && sa.AssignedOn.Date == currentDate.Date);
                    var rotaDetail = rotaDetails.FirstOrDefault(rd => rd.UserId == user.Id && rd.RotaDate.Date == currentDate.Date);
                    var leave = leaves.FirstOrDefault(l => l.UserId == user.Id &&
                        ((l.StartDate.HasValue && l.StartDate.Value.Date <= currentDate.Date && l.EndDate.HasValue && l.EndDate.Value.Date >= currentDate.Date)));

                    // If there is an approved leave, mark the day as "Off" and exclude from total hours
                    if (leave != null && leave.Status == "Approved")
                    {
                        return new DayRota
                        {
                            Day = currentDate.DayOfWeek,
                            IsOff = true,
                            ShiftName = "Off",
                            ShiftStartTime = TimeSpan.Zero,
                            ShiftEndTime = TimeSpan.Zero,
                            AvailableHours = 0,
                            LeaveType = leave.LeaveType,
                            LeaveStatus = leave.Status
                        };
                    }

                    // Check if the user has no shift (ShiftStartTime and ShiftEndTime are 00:00:00 in RotaDetail)
                    bool isNoShift = (rotaDetail != null && rotaDetail.ShiftStartTime == TimeSpan.Zero && rotaDetail.ShiftEndTime == TimeSpan.Zero);

                    // Determine the shift name and status
                    string shiftName;
                    bool isOff;

                    if (shiftAssignment == null)
                    {
                        shiftName = "Off";
                        isOff = true;
                    }
                    else if (isNoShift && currentDate.Date < DateTime.UtcNow.Date)
                    {
                        shiftName = "No Shift";
                        isOff = false;
                    }
                    else
                    {
                        shiftName = rotaDetail != null ? "Rota Shift" : shiftAssignment.Shift.ShiftName;
                        isOff = false;
                    }

                    return new DayRota
                    {
                        Day = currentDate.DayOfWeek,
                        IsOff = isOff,
                        ShiftName = shiftName,
                        ShiftStartTime = rotaDetail?.ShiftStartTime ?? shiftAssignment?.Shift?.FromTime ?? TimeSpan.Zero,
                        ShiftEndTime = rotaDetail?.ShiftEndTime ?? shiftAssignment?.Shift?.ToTime ?? TimeSpan.Zero,
                        AvailableHours = isNoShift ? 0 : (rotaDetail != null ? (rotaDetail.ShiftEndTime - rotaDetail.ShiftStartTime).TotalHours : 0),
                        LeaveType = leave?.LeaveType,
                        LeaveStatus = leave?.Status,
                        IsEditable = currentDate.Date >= DateTime.UtcNow.Date // Allow editing for current and future dates
                    };
                }).ToList()
            }).ToList();

            // Step 8: Create ViewModel
            var model = new RotaViewModel
            {
                WeekNumber = weekNumber,
                StartOfWeek = startOfWeek,
                EndOfWeek = endOfWeek,
                SelectedCountry = selectedCountry,
                CountryList = new List<SelectListItem>
        {
            new SelectListItem { Text = "India", Value = "India" },
            new SelectListItem { Text = "United Kingdom", Value = "United Kingdom" }
        },
                UserRotas = userRotas,
                TotalAvailableHoursForWeek = totalAvailableHoursForWeek,
                SelectedYear = selectedYear ?? DateTime.UtcNow.Year,
                SelectedMonth = selectedMonth ?? DateTime.UtcNow.Month,
                SelectedDay = selectedDay ?? DateTime.UtcNow.Day
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveRota([FromBody] List<RotaDetail> rotaDetails)
        {
            if (rotaDetails == null || !rotaDetails.Any())
            {
                return Json(new { success = false, message = "No data received." });
            }

            try
            {
                foreach (var rota in rotaDetails)
                {
                    var existingRota = await _context.StaffRota
                        .FirstOrDefaultAsync(r => r.UserId == rota.UserId && r.RotaDate == rota.RotaDate);

                    if (existingRota != null)
                    {
                        // Update only the fields that have been provided
                        if (rota.ShiftStartTime != TimeSpan.Zero)
                        {
                            existingRota.ShiftStartTime = rota.ShiftStartTime;
                        }

                        if (rota.ShiftEndTime != TimeSpan.Zero)
                        {
                            existingRota.ShiftEndTime = rota.ShiftEndTime;
                        }

                        // Recalculate available hours
                        existingRota.AvailableHours = (decimal)CalculateShiftHours(existingRota.ShiftStartTime, existingRota.ShiftEndTime);
                    }
                    else
                    {
                        // Add new rota if it doesn't exist
                        rota.AvailableHours = (decimal)CalculateShiftHours(rota.ShiftStartTime, rota.ShiftEndTime);
                        _context.StaffRota.Add(rota);
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private double CalculateShiftHours(TimeSpan fromTime, TimeSpan toTime)
        {
            if (toTime < fromTime)
            {
                // Handle shifts crossing midnight (e.g., 19:30 - 02:30)
                toTime = toTime.Add(TimeSpan.FromHours(24));
            }
            return (toTime - fromTime).TotalHours;
        }
    }
}