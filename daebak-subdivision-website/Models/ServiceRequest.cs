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

        [Column("REQUEST_TYPE")]
        [Required]
        public string RequestType { get; set; }

        [Column("LOCATION")]
        [Required]
        public string Location { get; set; }

        [Column("DESCRIPTION")]
        [Required]
        public string Description { get; set; }

        [Column("STATUS")]
        [Required]
        public string Status { get; set; } = "Pending"; // Default status

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("ASSIGNED_TO")]
        public int? AssignedTo { get; set; } // Nullable as per DB schema

        [Column("SCHEDULED_DATE")]
        public DateTime? ScheduledDate { get; set; }

        [Column("STAFF_NOTES")]
        public string? StaffNotes { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("UserId")]
        public virtual Homeowner Homeowner { get; set; }

        // Map AdminResponse to STAFF_NOTES
        [NotMapped]
        public string? AdminResponse
        {
            get => StaffNotes;
            set => StaffNotes = value;
        }

        // Map HouseNumber from Homeowner
        [NotMapped]
        public string? HouseNumber
        {
            get => Homeowner?.HouseNumber;
            set
            {
                if (Homeowner != null)
                {
                    Homeowner.HouseNumber = value;
                }
            }
        }
    }
}
