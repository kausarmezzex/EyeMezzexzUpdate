using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MezzexEye.Models
{
    public class RotaViewModel
    {
        internal int SelectedYear;
        internal int SelectedMonth;
        internal int SelectedDay;

        public int WeekNumber { get; set; }
        public DateTime StartOfWeek { get; set; }
        public DateTime EndOfWeek { get; set; }
        public string SelectedCountry { get; set; }
        public List<SelectListItem> CountryList { get; set; }
        public List<Shift> Shifts { get; set; }
        public int? SearchShiftId { get; set; }
        public List<UserRota> UserRotas { get; set; }
        public double TotalAvailableHoursForWeek { get; set; } // Add this property
    }


}
