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

        [Column("HOUSE_NUMBER")]
        public string? HouseNumber { get; set; }  // This property seems to be in your model but not in the DB schema

        [Column("LOCATION")]
        public string Location { get; set; }

        [Column("REQUEST_TYPE")]
        public string RequestType { get; set; }

        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Column("STATUS")]
        public string Status { get; set; } // Open, In Progress, Scheduled, Completed, Cancelled

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("ASSIGNED_TO")]
        public int? AssignedTo { get; set; } // Nullable as per DB schema

        [Column("SCHEDULED_DATE")]
        public DateTime? ScheduledDate { get; set; }

        [Column("STAFF_NOTES")]
        public string StaffNotes { get; set; }

        // Map AdminResponse to STAFF_NOTES since there's no explicit AdminResponse column
        public string AdminResponse
        {
            get { return StaffNotes; }
            set { StaffNotes = value; }
        }
    }
}
