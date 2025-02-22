using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EyeMezzexz.Models
{
    public class RotaDetail
    {
        [Key]
        public int RotaId { get; set; } // Primary Key

        [Required]
        public int UserId { get; set; } // FK to ApplicationUser
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Navigation property for user

        [Required]
        public DateTime RotaDate { get; set; } // Date for the rota

        public int? ShiftId { get; set; } // FK to Shift table
        [ForeignKey("ShiftId")]
        public Shift AssignedShift { get; set; } // Navigation property for shift details

        public TimeSpan ShiftStartTime { get; set; } // Start time of the shift
        public TimeSpan ShiftEndTime { get; set; } // End time of the shift

        public decimal AvailableHours { get; set; } // Total available hours for the day

        public bool IsOff { get; set; } // Whether the user is off for the day

        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifyOn { get; set; }
        public string? ModifyBy { get; set; }
    }
}
