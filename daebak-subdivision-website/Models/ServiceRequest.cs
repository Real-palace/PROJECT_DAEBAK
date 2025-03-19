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
        public string? HouseNumber { get; set; }  // Nullable as per DB schema

        [Column("REQUEST_TYPE")]
        public string RequestType { get; set; }

        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Column("STATUS")]
        public string Status { get; set; } // Open, In Progress, Closed

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [Column("ASSIGNED_TO")]
        public int? AssignedTo { get; set; } // Nullable as per DB schema
    }
}
