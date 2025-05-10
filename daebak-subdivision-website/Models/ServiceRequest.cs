using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("SERVICE_REQUESTS")] // Ensure it maps to the actual table name
    public class ServiceRequest
    {
        [Key]
        [Column("REQUEST_ID")]
        public int Id { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("LOCATION")]
        public string? Location { get; set; }

        [Required]
        [Column("REQUEST_TYPE")]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [Column("DESCRIPTION")]
        public string Description { get; set; } = string.Empty;

        [Column("STATUS")]
        public string? Status { get; set; } // Open, In Progress, Scheduled, Completed, Cancelled

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("ASSIGNED_TO")]
        public int? AssignedTo { get; set; }

        [Column("SCHEDULED_DATE")]
        public DateTime? ScheduledDate { get; set; }

        [Column("STAFF_NOTES")]
        public string? StaffNotes { get; set; }

        [NotMapped]
        public string? AdminResponse
        {
            get { return StaffNotes; }
            set { StaffNotes = value; }
        }

        // Navigation property for images
        public ICollection<RequestImage> Images { get; set; } = new List<RequestImage>();
    }
}
