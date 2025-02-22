namespace EyeMezzexz.Models
{
    public class UserComputer
    {
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ComputerId { get; set; }
        public Computer Computer { get; set; }
    }
}
