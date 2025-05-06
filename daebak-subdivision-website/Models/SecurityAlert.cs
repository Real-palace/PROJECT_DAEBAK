using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("SECURITY_ALERTS")]
    public class SecurityAlert
    {
        [Key]
        [Column("ALERT_ID")]
        public int AlertId { get; set; }

        [Required]
        [Column("TITLE")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [Column("CONTENT")]
        public string Content { get; set; }

        [Required]
        [Column("ALERT_TYPE")]
        [StringLength(20)]
        public string AlertType { get; set; }  // "info", "warning", "danger"

        [Column("EXPIRY_DATE")]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [Column("CREATED_BY_ID")]
        public int CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; }

        [Column("NOTIFY_APP")]
        public bool NotifyApp { get; set; }

        [Column("NOTIFY_EMAIL")]
        public bool NotifyEmail { get; set; }

        [Column("NOTIFY_SMS")]
        public bool NotifySms { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; }
    }
}