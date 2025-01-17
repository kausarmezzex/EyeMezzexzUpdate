using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MezzexEye.ViewModel
{
    public class ShiftAssignmentViewModel
    {
        public DateTime AssignedDate { get; set; } // Start date of the selected week
        public string AssignedWeekDisplay { get; set; } // Display format for the assigned week range

        public int SelectedWeekNumber { get; set; } // Selected week number (1 to 52)
        public List<SelectListItem> WeekNumbers { get; set; } = Enumerable.Range(1, 52)
            .Select(i => new SelectListItem { Text = $"Week {i}", Value = i.ToString() })
            .ToList(); // List of week numbers for dropdown

        public string SelectedCountry { get; set; } = "India"; // Selected country for filtering
        public List<SelectListItem> CountryList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Text = "India", Value = "India" },
            new SelectListItem { Text = "United Kingdom", Value = "United Kingdom" }
        };

        public List<UserShiftAssignment> UserShiftAssignments { get; set; } = new List<UserShiftAssignment>(); // Shift assignments per user
        public List<Shift> Shifts { get; set; } // List of shifts for dropdowns

        public IEnumerable<SelectListItem> ShiftSelectList =>
            Shifts?.Select(s => new SelectListItem { Value = s.ShiftId.ToString(), Text = s.ShiftName })
            ?? new List<SelectListItem>();

        public List<DayAvailableHours> DayWiseAvailableHours { get; set; } = new List<DayAvailableHours>();
        public int? SearchShiftId { get; internal set; }
    }

    public class UserShiftAssignment
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<DayShiftAssignment> DayWiseAssignments { get; set; } = new List<DayShiftAssignment>();
    }

    public class DayShiftAssignment
    {
        public DayOfWeek Day { get; set; }
        public List<int> ShiftIds { get; set; } = new List<int>(); // List to store multiple shift IDs
    }


    public class DayAvailableHours
    {
        public DayOfWeek Day { get; set; }
        public double AvailableHours { get; set; }
    }
}
