using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public class ComputerCategory
    {
        [Key]
        public int ComputerCategoryId { get; set; }
        public int ComputerId { get; set; } // Foreign key to Computer
        public int CategoryId { get; set; } // Foreign key to Category

        // Navigation properties
        public Computer Computer { get; set; }

        // Audit fields
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? ModifyOn { get; set; }
        public string? ModifyBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}