namespace MezzexEye.Models
{
    public class OfficeLeaveViewModel
    {
        public int? LeaveId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? LeaveType { get; set; } // Full Day, Half Day, Emergency, Planned, etc.
        public DateTime? StartDate { get; set; } // Leave start date
        public TimeSpan? StartTime { get; set; } // Applicable only for the UK
        public DateTime? EndDate { get; set; } // Leave end date
        public TimeSpan? EndTime { get; set; } // Applicable only for the UK
        public string? CountryName { get; set; } // To filter by country
        public string? Status { get; set; } // Approved, Pending, Rejected
        public string? Notes { get; set; } // Optional notes
        public string? LeavePaymentType { get; set; } // "Paid" or "Unpaid"
        public string? StatusChnageBy { get; set; } // Who changed the status
        public DateTime? StatusChnageOn { get; set; } // When the status was changed
        public string? AdminComment { get; set; } // Admin's comment during accept/reject
        public int? PaidLeaveDays { get; set; } // Days allocated as paid leave (India)
        public int? PaidLeaveHours { get; set; } // Hours allocated as paid leave (UK)
        public DateTime? LeaveCreatedOn { get; set; } // Date when the leave was created
        public DateTime? LeaveForDate { get; set; } // Date for time-based leave
    }
}
