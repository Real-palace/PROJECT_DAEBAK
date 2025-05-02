using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("STAFF")] // Ensure mapping to STAFF table
    public class Staff
    {
        [Key]
        [Column("STAFF_ID")]
        public int StaffId { get; set; }  // Maps to STAFF_ID

        [Column("USER_ID")]
        public int UserId { get; set; }  // Maps to USER_ID

        [Required]
        [Column("DEPARTMENT")]
        [StringLength(10)]
        public string Department { get; set; }  // Maps to DEPARTMENT column

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
