using System;
using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public class OfficeLeave
    {
        [Key]
        public int LeaveId { get; set; }

        [Required]
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; } // Navigation property

        [Required]
        [MaxLength(100)]
        public string LeaveType { get; set; } // Full Day, Half Day, Emergency, Planned, etc.


        public DateTime? StartDate { get; set; } // Leave start date

        public TimeSpan? StartTime { get; set; } // Applicable only for the UK


        public DateTime? EndDate { get; set; } // Leave end date

        public TimeSpan? EndTime { get; set; } // Applicable only for the UK

        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; } // To filter by country

        public string? Status { get; set; } // Approved, Pending, Rejected

        public string? Notes { get; set; } // Optional notes
    }
}
