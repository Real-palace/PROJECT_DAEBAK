namespace daebak_subdivision_website.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RequestType { get; set; }
        public string Description { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // Open, In Progress, Completed
    }
}
