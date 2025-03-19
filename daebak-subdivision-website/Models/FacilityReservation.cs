using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("FACILITY_RESERVATIONS")]
    public class FacilityReservation
    {
        [Key]
        [Column("RESERVATION_ID")]
        public int ReservationId { get; set; }

        [Required]
        [Column("FACILITY_ID")]
        public int FacilityId { get; set; }

        [Required]
        [Column("USER_ID")]
        public int UserId { get; set; }

        [Required]
        [Column("RESERVATION_DATE")]
        public DateTime ReservationDate { get; set; }

        [Required]
        [Column("STATUS")]
        public string Status { get; set; } = "Pending";

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("FacilityId")]
        public Facility Facility { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
