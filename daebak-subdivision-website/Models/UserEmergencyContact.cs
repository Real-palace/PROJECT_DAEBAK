using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("USER_EMERGENCY_CONTACTS")]
    public class UserEmergencyContact
    {
        [Key]
        [Column("CONTACT_ID")]
        public int ContactId { get; set; }
        
        [Required]
        [Column("USER_ID")]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [Required]
        [Column("CONTACT_NAME")]
        [StringLength(100)]
        public string ContactName { get; set; }
        
        [Required]
        [Column("RELATIONSHIP")]
        [StringLength(50)]
        public string Relationship { get; set; }
        
        [Required]
        [Column("CONTACT_PHONE")]
        [StringLength(20)]
        public string ContactPhone { get; set; }
        
        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }
        
        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; }
    }
}