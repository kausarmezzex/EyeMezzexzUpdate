using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public class UserLog
    {
        [Key]
        public int LogId { get; set; } // Primary Key
        public string? UserId { get; set; } // User performing the action
        public string? Action { get; set; } // Page Visit, Update, Delete, etc.
        public string? EntityName { get; set; } // Entity involved (e.g., UserAccountDetails)
        public string? PropertyName { get; set; } // Updated property name
        public string? OldValue { get; set; } // Previous value
        public string? NewValue { get; set; } // Updated value
        public string? LogLevel { get; set; } // Information, Warning, Error
        public string? URL { get; set; } // Requested URL
        public DateTime? Timestamp { get; set; } // Timestamp of the action
        public string? Details { get; set; } // Additional details
    }
}
