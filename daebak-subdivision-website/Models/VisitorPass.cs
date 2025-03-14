namespace daebak_subdivision_website.Models
{
    public class VisitorPass
    {
        public int Id { get; set; }
        public string VisitorName { get; set; }
        public string Purpose { get; set; }
        public DateTime VisitDate { get; set; }
        public int UserId { get; set; } // Assuming a relation to User
    }
}
