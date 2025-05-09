using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("REQUEST_IMAGES")]
    public class RequestImage
    {
        [Key]
        [Column("IMAGE_ID")]
        public int Id { get; set; }

        [Column("REQUEST_ID")]
        public int RequestId { get; set; }

        [Column("IMAGE_PATH")]
        [Required]
        public string ImagePath { get; set; }

        [Column("UPLOADED_AT")]
        public DateTime UploadedAt { get; set; }

        // Navigation property
        [ForeignKey("RequestId")]
        public virtual ServiceRequest ServiceRequest { get; set; }
    }
} 