namespace daebak_subdivision_website.Models
{
    public class ForumThread
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public List<ForumPost> Posts { get; set; } = new List<ForumPost>();
    }
}
