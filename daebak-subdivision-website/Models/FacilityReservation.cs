namespace daebak_subdivision_website.Models
{
    public class FacilityReservation
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int UserId { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
    }
}
