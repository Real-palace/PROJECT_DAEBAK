namespace daebak_subdivision_website.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Comments { get; set; }
        public int Rating { get; set; } // Example: 1 to 5 stars
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
