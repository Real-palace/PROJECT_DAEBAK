using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace daebak_subdivision_website.Models
{
    [Table("VEHICLE_REGISTRATION")]
    public class VehicleRegistration
    {
        [Key]
        [Column("VEHICLE_ID")]
        public int RegistrationId { get; set; }

        [Required]
        [Column("USER_ID")]
        public int OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }

        [Required]
        [Column("PLATE_NUMBER")]
        [StringLength(20)]
        public string PlateNumber { get; set; }

        [Required]
        [Column("VEHICLE_MAKE")]
        [StringLength(50)]
        public string VehicleMake { get; set; }

        [Required]
        [Column("VEHICLE_MODEL")]
        [StringLength(50)]
        public string VehicleModel { get; set; }

        [Required]
        [Column("VEHICLE_COLOR")]
        [StringLength(30)]
        public string VehicleColor { get; set; }

        [Required]
        [Column("VEHICLE_YEAR")]
        public int VehicleYear { get; set; }

        [Required]
        [Column("VEHICLE_TYPE")]
        [StringLength(30)]
        public string VehicleType { get; set; }

        [Column("VEHICLE_PHOTO_PATH")]
        [StringLength(255)]
        public string VehiclePhotoPath { get; set; }

        [Column("IS_PRIMARY")]
        public bool IsPrimary { get; set; }

        [Required]
        [Column("STATUS")]
        [StringLength(10)]
        public string Status { get; set; } // "Pending", "Approved", "Denied"

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; }

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; }
        
        [Column("REGISTRATION_DATE")]
        public DateTime RegistrationDate { get; set; }
    }
}
