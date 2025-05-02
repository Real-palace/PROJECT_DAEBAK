using System;
using System.ComponentModel.DataAnnotations;

namespace daebak_subdivision_website.Models
{
    public class UserViewModel
    {
        public int UserId { get; set; }  // Changed from Id to UserId to match controller

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; }

        [Required, StringLength(50)]
        public string LastName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(10)]
        public string HouseNumber { get; set; }

        [StringLength(10)]
        public string Department { get; set; }

        [Required, StringLength(10)]
        public string Role { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
