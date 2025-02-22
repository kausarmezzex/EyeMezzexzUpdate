namespace MezzexEye.Models
{
    public class DayRota
    {
        internal double AvailableHours;
        internal bool IsEditable;

        public DayOfWeek Day { get; set; }
        public bool IsOff { get; set; }
        public string? ShiftName { get; set; }
        public TimeSpan ShiftStartTime { get; set; } // Nullable TimeSpan
        public TimeSpan ShiftEndTime { get; set; }   // Nullable TimeSpan
        public string? LeaveType { get; set; }
        public string? LeaveStatus { get; set; }
    }


}
