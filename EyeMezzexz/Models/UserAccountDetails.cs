using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EyeMezzexz.Models
{
    public class UserAccountDetails
    {
        [Key]
        public int AccountDetailsId { get; set; } // Primary Key

        // Foreign Key for ApplicationUser
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? ApplicationUser { get; set; } // Navigation Property

        // Other properties specific to UserAccountDetails
        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Payroll number must be a positive value.")]
        public int PayrollNumber { get; set; }

        public string EmployeeCode { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        [Required]
        [StringLength(50)]
        public string AgreementType { get; set; } // Added AgreementType

        public string? Address { get; set; }

        public string? Zipcode { get; set; }

        public string? ExtraInfo { get; set; }

/*        public int? AuthenticLeave { get; set; }*/

        public bool IsFixedHours { get; set; }

        public bool IsFixedSalary { get; set; }

        public decimal? FixedSalaryAmount { get; set; }

        public int? AgreedHours { get; set; }

        public int? PayrollHours { get; set; }

        public decimal? PayrollHoursRate { get; set; }

        public int? CashHours { get; set; }

        public decimal? CashHoursRate { get; set; }

        public bool IsPayBack { get; set; }

        public string? PayBackTo { get; set; }

        public decimal? PayBackAmount { get; set; }

        public int? HolidayDaysYearly { get; set; }

        public int? HolidayHoursYearly { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ContractStartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ContractEndDate { get; set; }
        public string? NINumber { get; set; }
        public bool IsManager { get; set; }
        public List<string>? DocumentNames { get; set; } // Multiple document names

        public List<byte[]>? Documents { get; set; } // File content as byte arrays
    }
}
