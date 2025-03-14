namespace daebak_subdivision_website.Models
{
    public class VehicleRegistration
    {
        public int Id { get; set; }
        public string VehicleNumber { get; set; }
        public string OwnerName { get; set; }
        public string VehicleType { get; set; }
        public int UserId { get; set; } // Assuming a relation to User
    }
}
