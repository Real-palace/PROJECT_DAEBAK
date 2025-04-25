using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("EVENTS")]
    public class Event
    {
        [Key]
        [Column("EVENT_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("TITLE")]
        [StringLength(100)]
        public string Title { get; set; }

        // Made Description optional by removing the [Required] attribute
        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Required]
        [Column("START_DATE")]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("END_DATE")]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("LOCATION")]
        [StringLength(100)]
        public string Location { get; set; }

        [Column("CREATED_BY")]
        [Display(Name = "Created By")]
        public int? CreatedById { get; set; }

        [Column("CREATED_AT")]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        [Display(Name = "Last Updated")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CreatedById")]
        public User CreatedBy { get; set; }
    }
}
