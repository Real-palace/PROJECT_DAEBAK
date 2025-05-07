using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("FEEDBACK_RESPONSES")]
    public class FeedbackResponse
    {
        [Key]
        [Column("RESPONSE_ID")]
        public int ResponseId { get; set; }

        [Column("FEEDBACK_ID")]
        public int FeedbackId { get; set; }

        [Column("RESPONSE_TEXT")]
        [Required]
        public string ResponseText { get; set; }

        [Column("RESPONDED_BY")]
        [Required]
        public string RespondedBy { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public virtual Feedback Feedback { get; set; }
    }
}