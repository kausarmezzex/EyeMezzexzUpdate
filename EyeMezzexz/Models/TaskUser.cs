namespace EyeMezzexz.Models
{
    public class TaskUser
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TaskNames Task { get; set; }
        
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}