using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("FEEDBACK")]
    public class Feedback
    {
        [Key]
        [Column("FEEDBACK_ID")]
        public int FeedbackId { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("HOUSE_NUMBER")]
        public string HouseNumber { get; set; }

        [Column("FEEDBACK_TYPE")]
        [Required]
        public string FeedbackType { get; set; }

        [Column("DESCRIPTION")]
        [Required]
        public string Description { get; set; }

        [Column("STATUS")]
        public string Status { get; set; } = "Submitted";

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
