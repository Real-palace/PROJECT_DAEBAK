namespace daebak_subdivision_website.Models
{
    public class Poll
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public List<PollResponse> Responses { get; set; } = new List<PollResponse>();
    }
}
