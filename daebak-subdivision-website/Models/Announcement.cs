using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("ANNOUNCEMENTS")]
    public class Announcement
    {
        public Announcement()
        {
            TITLE = string.Empty;
            CONTENT = string.Empty;
            CREATED_AT = DateTime.Now;
            UPDATED_AT = DateTime.Now;
            Category = "NOTICE";
            CategoryColor = "blue";
        }

        [Key]
        public int ANNOUNCEMENT_ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TITLE { get; set; } = string.Empty;

        [Required]
        public string CONTENT { get; set; } = string.Empty;

        public int? CREATED_BY { get; set; }

        public DateTime CREATED_AT { get; set; }
        
        public DateTime UPDATED_AT { get; set; }

        [ForeignKey("CREATED_BY")]
        public virtual User? CreatedByUser { get; set; }

        [NotMapped]
        public string Category { get; set; }

        [NotMapped]
        public string CategoryColor { get; set; }
        
        // Add alias properties to match view naming conventions
        [NotMapped]
        public string Title { 
            get { return TITLE; } 
            set { TITLE = value; }
        }

        [NotMapped]
        public string Content { 
            get { return CONTENT; } 
            set { CONTENT = value; }
        }

        [NotMapped]
        public DateTime CreatedAt { 
            get { return CREATED_AT; } 
            set { CREATED_AT = value; }
        }
    }
}