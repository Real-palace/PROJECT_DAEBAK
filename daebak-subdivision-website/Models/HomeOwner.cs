using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("HOMEOWNERS")] // Ensures EF maps to the correct table
    public class Homeowner
    {
        [Column("HOMEOWNER_ID")]
        public int HomeownerId { get; set; }

        [Column("USER_ID")]
        public int UserId { get; set; }

        [Column("HOUSE_NUMBER")]
        public string? HouseNumber { get; set; } = null;

        [ForeignKey("UserId")]
        public virtual required User User { get; set; }
    }
}
