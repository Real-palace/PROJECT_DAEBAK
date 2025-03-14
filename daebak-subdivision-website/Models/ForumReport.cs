namespace daebak_subdivision_website.Models
{
    public class ForumReport
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public DateTime ReportedAt { get; set; } = DateTime.Now;

        // Foreign Key
        public int ForumPostId { get; set; }
        public ForumPost ForumPost { get; set; }
    }
}
