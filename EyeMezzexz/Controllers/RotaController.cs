using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Data;
using System.Globalization;
using OfficeOpenXml;
using System.IO;
namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RotaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RotaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Rota/UserRota/{userId}
        [HttpGet("UserRota/{userId}")]
        public async Task<ActionResult<IEnumerable<RotaResponse>>> GetUserRota(
            int userId,
            int? weekNumber = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                DateTime startOfWeek;
                DateTime endOfWeek;

                if (startDate.HasValue && endDate.HasValue)
                {
                    // Use the provided date range
                    startOfWeek = startDate.Value;
                    endOfWeek = endDate.Value;
                }
                else if (weekNumber.HasValue)
                {
                    // Validate week number (1-53)
                    if (weekNumber < 1 || weekNumber > 53)
                    {
                        return BadRequest("Invalid week number. It must be between 1 and 53.");
                    }
                    startOfWeek = ISOWeek.ToDateTime(DateTime.UtcNow.Year, weekNumber.Value, DayOfWeek.Monday);
                    endOfWeek = startOfWeek.AddDays(6);
                }
                else
                {
                    // Default to the current week
                    startOfWeek = ISOWeek.ToDateTime(DateTime.UtcNow.Year, ISOWeek.GetWeekOfYear(DateTime.UtcNow), DayOfWeek.Monday);
                    endOfWeek = startOfWeek.AddDays(6);
                }

                var userRotaDetails = await _context.StaffRota
                    .Where(rd => rd.UserId == userId && rd.RotaDate >= startOfWeek && rd.RotaDate <= endOfWeek)
                    .Include(rd => rd.User)
                    .Include(rd => rd.AssignedShift)
                    .ToListAsync();

                var shiftAssignments = await _context.ShiftAssigned
                    .Include(sa => sa.Shift)
                    .Where(sa => sa.UserId == userId && sa.AssignedOn >= startOfWeek && sa.AssignedOn <= endOfWeek)
                    .ToListAsync();

                if (!userRotaDetails.Any() && !shiftAssignments.Any())
                {
                    return NotFound("No rota details or shift assignments found for the specified user and date range.");
                }

                // Convert userRotaDetails to List<RotaResponse>
                var rotaDetailsList = userRotaDetails.Select(rd => new RotaResponse
                {
                    RotaDate = rd.RotaDate,
                    RotaDay = rd.RotaDate.ToString("dddd"), // Convert date to day name
                    ShiftStartTime = rd.ShiftStartTime,
                    ShiftEndTime = rd.ShiftEndTime,
                    ShiftName = rd.AssignedShift?.ShiftName ?? "No Shift",
                    AssignedShift = rd.AssignedShift
                }).ToList();

                // Convert shiftAssignments to List<RotaResponse>
                var shiftAssignmentsList = shiftAssignments.Select(sa => new RotaResponse
                {
                    RotaDate = sa.AssignedOn,
                    RotaDay = sa.AssignedOn.ToString("dddd"), // Convert date to day name
                    ShiftStartTime = sa.Shift?.FromTime,
                    ShiftEndTime = sa.Shift?.ToTime,
                    ShiftName = sa.Shift?.ShiftName ?? "No Shift",
                    AssignedShift = sa.Shift
                }).ToList();

                // Merge both lists
                var result = rotaDetailsList.Concat(shiftAssignmentsList).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserRota: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }
        [HttpGet("ExportToExcel/{userId}")]
        public async Task<IActionResult> ExportRotaToExcel(int userId, DateTime? startDate = null, DateTime? endDate = null, int? weekNumber = null)
        {
            try
            {
                DateTime startOfWeek;
                DateTime endOfWeek;

                if (startDate.HasValue && endDate.HasValue)
                {
                    startOfWeek = startDate.Value;
                    endOfWeek = endDate.Value;
                }
                else if (weekNumber.HasValue)
                {
                    if (weekNumber < 1 || weekNumber > 53)
                    {
                        return BadRequest("Invalid week number. It must be between 1 and 53.");
                    }
                    startOfWeek = ISOWeek.ToDateTime(DateTime.UtcNow.Year, weekNumber.Value, DayOfWeek.Monday);
                    endOfWeek = startOfWeek.AddDays(6);
                }
                else
                {
                    startOfWeek = ISOWeek.ToDateTime(DateTime.UtcNow.Year, ISOWeek.GetWeekOfYear(DateTime.UtcNow), DayOfWeek.Monday);
                    endOfWeek = startOfWeek.AddDays(6);
                }

                var userRotaDetails = await _context.StaffRota
                    .Where(rd => rd.UserId == userId && rd.RotaDate >= startOfWeek && rd.RotaDate <= endOfWeek)
                    .Include(rd => rd.User)
                    .Include(rd => rd.AssignedShift)
                    .ToListAsync();

                var shiftAssignments = await _context.ShiftAssigned
                    .Include(sa => sa.Shift)
                    .Where(sa => sa.UserId == userId && sa.AssignedOn >= startOfWeek && sa.AssignedOn <= endOfWeek)
                    .ToListAsync();

                if (!userRotaDetails.Any() && !shiftAssignments.Any())
                {
                    return NotFound("No data found for the specified filters.");
                }

                var rotaDetailsList = userRotaDetails.Select(rd => new RotaResponse
                {
                    RotaDate = rd.RotaDate,
                    RotaDay = rd.RotaDate.ToString("dddd"),
                    ShiftStartTime = rd.ShiftStartTime,
                    ShiftEndTime = rd.ShiftEndTime,
                    ShiftName = rd.AssignedShift?.ShiftName ?? "No Shift",
                    AssignedShift = rd.AssignedShift
                }).ToList();

                var shiftAssignmentsList = shiftAssignments.Select(sa => new RotaResponse
                {
                    RotaDate = sa.AssignedOn,
                    RotaDay = sa.AssignedOn.ToString("dddd"),
                    ShiftStartTime = sa.Shift?.FromTime,
                    ShiftEndTime = sa.Shift?.ToTime,
                    ShiftName = sa.Shift?.ShiftName ?? "No Shift",
                    AssignedShift = sa.Shift
                }).ToList();

                var rotaData = rotaDetailsList.Concat(shiftAssignmentsList).ToList();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // ✅ Fix applied here
                // **EXCEL FILE GENERATION STARTS HERE**
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("RotaDetails");

                    worksheet.Cells[1, 1].Value = "Date";
                    worksheet.Cells[1, 2].Value = "Day";
                    worksheet.Cells[1, 3].Value = "Shift Start Time";
                    worksheet.Cells[1, 4].Value = "Shift End Time";
                    worksheet.Cells[1, 5].Value = "Shift Name";

                    int row = 2;
                    foreach (var item in rotaData)
                    {
                        worksheet.Cells[row, 1].Value = item.RotaDate.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 2].Value = item.RotaDay;
                        worksheet.Cells[row, 3].Value = item.ShiftStartTime?.ToString(@"hh\:mm");
                        worksheet.Cells[row, 4].Value = item.ShiftEndTime?.ToString(@"hh\:mm");
                        worksheet.Cells[row, 5].Value = item.ShiftName;
                        row++;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RotaDetails.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting to Excel: {ex.Message}");
                return StatusCode(500, "An error occurred while exporting data to Excel.");
            }
        }

        public class RotaResponse
        {
            public DateTime RotaDate { get; set; } // Original date
            public string RotaDay { get; set; }    // Day of the week (e.g., Monday, Tuesday)
            public TimeSpan? ShiftStartTime { get; set; }
            public TimeSpan? ShiftEndTime { get; set; }
            public string ShiftName { get; set; }
            public Shift AssignedShift { get; set; }
        }
    }
}
