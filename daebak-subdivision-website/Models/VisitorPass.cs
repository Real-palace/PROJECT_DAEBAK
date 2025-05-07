using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("VISITOR_PASSES")]
    public class VisitorPass
    {
        [Key]
        [Column("PASS_ID")]
        public int PassId { get; set; }

        [Required]
        [Column("USER_ID")]
        public int RequestedById { get; set; }

        [ForeignKey("RequestedById")]
        public virtual User RequestedBy { get; set; }

        [Required]
        [Column("VISITOR_NAME")]
        [StringLength(100)]
        public string VisitorName { get; set; }

        [Required]
        [Column("VISITOR_PHONE")]
        [StringLength(20)]
        public string VisitorPhone { get; set; }

        [Required]
        [Column("VISIT_DATE")]
        public DateTime VisitDate { get; set; }

        [Required]
        [Column("VISIT_TIME")]
        public TimeSpan VisitTime { get; set; }

        [Required]
        [Column("VISIT_PURPOSE")]
        [StringLength(50)]
        public string Purpose { get; set; }

        [Column("VEHICLE_PLATE")]
        [StringLength(20)]
        public string VehiclePlate { get; set; }

        [Column("VISIT_NOTES")]
        public string Notes { get; set; }

        [Required]
        [Column("STATUS")]
        [StringLength(10)]
        public string Status { get; set; } // "Pending", "Approved", "Denied"

        [Column("PASS_CODE")]
        [StringLength(20)]
        public string PassCode { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; }
    }
}
