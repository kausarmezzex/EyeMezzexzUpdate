using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public class TaskAssignmentRequest
    {
        public int TaskId { get; set; } // ID of the assigned task
        public TimeSpan? AssignedDuration { get; set; }  // Store as TimeSpan
        public int TargetQuantity { get; set; } // Target quantity for the task
        // New properties
        public List<int> ComputerIds { get; set; } = new List<int>(); // List of computer IDs (for UK tasks)
        public string Country { get; set; } // Country for the task (e.g., "India", "UK")
        // Audit properties
        [Required]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

    }
}
