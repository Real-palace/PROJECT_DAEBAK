using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daebak_subdivision_website.Models;
using System.Linq;

namespace daebak_subdivision_website.Controllers
{
    [Authorize(Roles = "HOMEOWNER")]
    public class HomeownerController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HomeownerController> _logger;

        public HomeownerController(ApplicationDbContext dbContext, ILogger<HomeownerController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning($"Dashboard access failed. User '{username}' not found.");
                return RedirectToAction("Index", "Home");
            }

            // Set ViewBag properties needed by the Dashboard view
            ViewBag.FirstName = $"{user.FirstName}";
            ViewBag.DueBills = 2; // You can replace with actual data from your database
            ViewBag.EventCount = 3; // You can replace with actual data from your database
            ViewBag.RequestCount = 1; // You can replace with actual data from your database

            // Optional: Add sample events data for the calendar
            ViewBag.Events = new[]
            {
                new { title = "Homeowners Meeting", start = "2025-04-15", color = "#FF9AA2" },
                new { title = "Facility Maintenance", start = "2025-04-20", color = "#B5EAD7" },
                new { title = "Community Clean-Up", start = "2025-04-25", color = "#C7CEEA" }
            };

            return View();
        }

        public IActionResult Billing()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";

            return View();
        }

        public IActionResult Facilities()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";

            return View();
        }

        public IActionResult Services()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";

            return View();
        }

        public IActionResult Security()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.UserName = $"{user.FirstName} {user.LastName}";

            return View();
        }

        public IActionResult Profile()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning($"Profile access failed. User '{username}' not found.");
                return RedirectToAction("Index", "Home");
            }

            // Create a model for the profile view
            var model = new UserProfileViewModel
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                HouseNumber = user.HouseNumber ?? string.Empty,
                ProfilePicture = user.ProfilePicture ?? "/images/profile/default.jpg",
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            ViewBag.UserName = $"{model.FirstName} {model.LastName}";
            ViewBag.MemberSince = model.CreatedAt.ToString("MMMM d, yyyy");

            _logger.LogInformation($"Profile loaded for user {model.Username} with house number {model.HouseNumber}");

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning($"UpdateProfile failed. User '{username}' not found.");
                return RedirectToAction("Index", "Home");
            }

            // Explicitly clear password validation errors if no password change was requested
            if (string.IsNullOrEmpty(model.NewPassword) && string.IsNullOrEmpty(model.CurrentPassword) && string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                // Remove any password-related validation errors
                ModelState.Remove("CurrentPassword");
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmNewPassword");
            }

            // Collect all validation errors instead of returning immediately
            bool hasValidationErrors = false;

            // Check for duplicate username (only if changed)
            if (user.Username != model.Username)
            {
                var existingUserWithSameUsername = _dbContext.Users.FirstOrDefault(u => u.Username == model.Username);
                if (existingUserWithSameUsername != null)
                {
                    ModelState.AddModelError("Username", "This username is already taken. Please choose a different one.");
                    _logger.LogWarning("Username already exists during profile update attempt: {Username}", model.Username);
                    hasValidationErrors = true;
                }
            }

            // Check for duplicate email (only if changed)
            if (user.Email != model.Email)
            {
                var existingUserWithSameEmail = _dbContext.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existingUserWithSameEmail != null)
                {
                    ModelState.AddModelError("Email", "This email address is already registered. Please use a different one.");
                    _logger.LogWarning("Email already exists during profile update attempt: {Email}", model.Email);
                    hasValidationErrors = true;
                }
            }

            // Check if the ModelState is valid after our validations
            if (!ModelState.IsValid || hasValidationErrors)
            {
                _logger.LogWarning("Profile update validation failed for user {Username}.", user.Username);

                // Set ViewBag data for the view
                ViewBag.UserName = $"{user.FirstName} {user.LastName}";
                ViewBag.MemberSince = user.CreatedAt.ToString("MMMM d, yyyy");

                return View("Profile", model);
            }

            // Handle password change if provided - collect all password validation errors at once
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                bool hasPasswordErrors = false;

                // Check if current password is provided
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to change your password.");
                    hasPasswordErrors = true;
                }
                else if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                {
                    // Validate current password (only if provided)
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    hasPasswordErrors = true;
                }

                // Check if new password meets requirements
                var passwordRegex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
                if (!passwordRegex.IsMatch(model.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 8 characters with at least 1 uppercase letter, 1 number, and 1 special character.");
                    hasPasswordErrors = true;
                }

                // Check if passwords match
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("ConfirmNewPassword", "New password and confirmation password do not match.");
                    hasPasswordErrors = true;
                }

                // Return with all validation errors at once
                if (hasPasswordErrors)
                {
                    _logger.LogWarning("Password validation failed during profile update for user {Username}.", user.Username);

                    // Set ViewBag data for the view
                    ViewBag.UserName = $"{user.FirstName} {user.LastName}";
                    ViewBag.MemberSince = user.CreatedAt.ToString("MMMM d, yyyy");

                    return View("Profile", model);
                }

                // All validations passed, update the password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _logger.LogInformation("Password updated successfully for user {Username}.", user.Username);
            }

            // Update user information
            user.Username = model.Username;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.UpdatedAt = DateTime.Now;

            try
            {
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                _logger.LogInformation("Profile updated successfully for user {Username}.", user.Username);

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {Username}.", user.Username);
                ModelState.AddModelError(string.Empty, "An error occurred while updating your profile.");

                // Set ViewBag data for the view
                ViewBag.UserName = $"{user.FirstName} {user.LastName}";
                ViewBag.MemberSince = user.CreatedAt.ToString("MMMM d, yyyy");

                return View("Profile", model);
            }
        }
    }
}