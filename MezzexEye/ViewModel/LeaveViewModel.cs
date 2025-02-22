using System.ComponentModel.DataAnnotations;

namespace MezzexEye.ViewModel
{
    public class LeaveViewModel
    {
        internal string FirstName;
        internal string LastName;

        public int LeaveId { get; set; }

        // 🟢 User Account Details को Map करना
        public UserAccountViewModel UserAccount { get; set; }

        [Required]
        [MaxLength(100)]
        public string LeaveType { get; set; }

        public DateTime? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan? EndTime { get; set; }

        [MaxLength(100)]
        public string? CountryName { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? LeavePaymentType { get; set; }
        public string? StatusChangeBy { get; set; }
        public DateTime? StatusChangeOn { get; set; }

        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifyOn { get; set; }
        public string? ModifyBy { get; set; }

        public string? AdminComment { get; set; }
        public int? PaidLeaveDays { get; set; }
        public int? PaidLeaveHours { get; set; }
    }
}
