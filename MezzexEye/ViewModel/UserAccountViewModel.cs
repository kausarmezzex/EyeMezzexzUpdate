namespace MezzexEye.ViewModel
{
    public class UserAccountViewModel
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string EmployeeCode { get; set; }
            
        // 🟢 Required Fields for Leave Page
        public int? RemainingYearlyHours { get; set; }  // सालाना बचे हुए घंटे
        public int? RemainingYearlyLeave { get; set; }  // सालाना बचे हुए छुट्टियाँ
    }
}
