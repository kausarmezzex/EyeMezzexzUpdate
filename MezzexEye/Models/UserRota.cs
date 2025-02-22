namespace MezzexEye.Models
{
    public class UserRota
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<DayRota> DayWiseRota { get; set; }
    }

}
