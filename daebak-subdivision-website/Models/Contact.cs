using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("CONTACTS")]
    public class EmergencyContact
    {
        [Key]
        [Column("CONTACT_ID")]
        public int ContactId { get; set; }

        [Required]
        [Column("NAME")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("POSITION")]
        [StringLength(100)]
        public string Position { get; set; }
        
        [Required]
        [Column("CONTACT_INFO")]
        [StringLength(255)]
        public string ContactInfo { get; set; }
        
        [Column("DESCRIPTION")]
        public string Description { get; set; }
        
        [Column("ICON_CLASS")]
        [StringLength(50)]
        public string IconClass { get; set; }
        
        [Column("BACKGROUND_COLOR")]
        [StringLength(50)]
        public string BackgroundColor { get; set; }
        
        [Column("ICON_COLOR")]
        [StringLength(20)]
        public string IconColor { get; set; }
        
        [Column("DISPLAY_ORDER")]
        public int DisplayOrder { get; set; }
    }
}