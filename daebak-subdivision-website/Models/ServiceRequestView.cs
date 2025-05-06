using System;

namespace daebak_subdivision_website.Models
{
    public class ServiceRequestView
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string HouseNumber { get; set; }
        public string Location { get; set; }
        public string RequestType { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? AssignedTo { get; set; }
        public string RequestedBy { get; set; }
        public string AssignedToName { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string StaffNotes { get; set; }
    }
}
