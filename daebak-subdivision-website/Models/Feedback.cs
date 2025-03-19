using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("FEEDBACK")] // Ensure table name matches database
    public class Feedback
    {
        [Key]
        [Column("FEEDBACK_ID")] // Matches DB column name
        public int FeedbackId { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("HOUSE_NUMBER")]
        public string HouseNumber { get; set; }

        [Column("FEEDBACK_TYPE")]
        public string FeedbackType { get; set; }

        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Column("STATUS")]
        public string Status { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual User User { get; set; }
    }
}
