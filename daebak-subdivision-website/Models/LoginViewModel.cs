using System.ComponentModel.DataAnnotations;

namespace daebak_subdivision_website.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty; // Default value

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // Default value
    }
}
