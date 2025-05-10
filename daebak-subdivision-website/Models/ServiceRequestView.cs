using System;
using System.ComponentModel.DataAnnotations;

namespace daebak_subdivision_website.Models
{
    public class ServiceRequestView
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Location { get; set; }

        [Required]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? AssignedTo { get; set; }
        public string? RequestedBy { get; set; }
        public string? AssignedToName { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string? StaffNotes { get; set; }
    }
}
