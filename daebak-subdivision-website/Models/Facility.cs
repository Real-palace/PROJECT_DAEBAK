using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace daebak_subdivision_website.Models
{
    [Table("FACILITIES")]
    public class Facility
    {
        [Key]
        [Column("FACILITY_ID")]
        public int FacilityId { get; set; }

        [Required]
        [Column("NAME")]
        public string Name { get; set; }

        [Column("DESCRIPTION")]
        public string Description { get; set; }

        [Column("CAPACITY")]
        public int Capacity { get; set; }

        [Column("STATUS")]
        public string Status { get; set; } = "Available";

        // Navigation Property
        public ICollection<FacilityReservation> Reservations { get; set; }
    }
}
