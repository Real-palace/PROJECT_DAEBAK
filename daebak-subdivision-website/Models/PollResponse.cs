namespace daebak_subdivision_website.Models
{
    public class PollResponse
    {
        public int Id { get; set; }
        public string ResponseText { get; set; }
        public int PollId { get; set; }

        // Navigation property
        public Poll Poll { get; set; }
    }
}
