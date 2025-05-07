using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace daebak_subdivision_website.Models
{
    [Table("USERS")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("USER_ID")]
        public int UserId { get; set; }

        [Required, StringLength(50)]
        [Column("USERNAME")]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(255)]
        [Column("PASSWORD_HASH")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(10)]
        [Column("ROLE")]
        public string Role { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("FIRST_NAME")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Column("LAST_NAME")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("PHONE_NUMBER")]
        public string? PhoneNumber { get; set; }

        [NotMapped] // This property is not mapped to a database column
        public string? ProfilePicture { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("UPDATED_AT")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property for Homeowner
        public virtual Homeowner? Homeowner { get; set; }

        // Navigation property for Staff
        public virtual Staff? Staff { get; set; }

        // Virtual property to get HouseNumber from Homeowner
        [NotMapped]
        public string? HouseNumber
        {
            get => Homeowner?.HouseNumber;
            set
            {
                // If Homeowner exists, update its HouseNumber
                if (Homeowner != null)
                {
                    Homeowner.HouseNumber = value ?? string.Empty;
                }
                // Property setter needed for model binding in form submissions
            }
        }

        // Virtual property to get Department from Staff
        [NotMapped]
        public string? Department
        {
            get => Staff?.Department;
            set
            {
                // If Staff exists, update its Department
                if (Staff != null)
                {
                    Staff.Department = value ?? string.Empty;
                }
                // Property setter needed for model binding in form submissions
            }
        }
    }
}