using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("ADMINS")] // Ensure mapping to ADMINS table
    public class Admin
    {
        [Key]
        [Column("ADMIN_ID")]
        public int AdminId { get; set; }  // Maps to ADMIN_ID

        [Column("USER_ID")]
        public int UserId { get; set; }  // Maps to USER_ID

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
