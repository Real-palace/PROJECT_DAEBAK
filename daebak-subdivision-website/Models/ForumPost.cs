namespace daebak_subdivision_website.Models
{
    public class ForumPost
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime PostedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int ForumThreadId { get; set; }
        public ForumThread ForumThread { get; set; }
    }
}
