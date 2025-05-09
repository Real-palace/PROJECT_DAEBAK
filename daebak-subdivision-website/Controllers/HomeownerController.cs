using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace daebak_subdivision_website.Controllers
{
    [Authorize(Roles = "HOMEOWNER")]
    public class HomeownerController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeownerController> _logger;

        public HomeownerController(ApplicationDbContext context, ILogger<HomeownerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            // Get current user ID
            var userId = User.FindFirst("UserId")?.Value;
            var username = User.Identity?.Name;

            var user = _context.Users
                .Include(u => u.Homeowner)
                .AsNoTracking()
                .FirstOrDefault(u => u.Username == username);

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

            // Fetch most recent announcements for the dashboard
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CREATED_AT)
                .Select(a => new
                {
                    Title = a.TITLE,
                    Content = a.CONTENT,
                    Category = a.Category,
                    CategoryColor = a.CategoryColor,
                    Date = a.CREATED_AT
                })
                .Take(3)
                .ToListAsync();

            ViewBag.Announcements = announcements;

            // Fetch all announcements for the modal
            var allAnnouncements = await _context.Announcements
                .OrderByDescending(a => a.CREATED_AT)
                .Select(a => new
                {
                    Title = a.TITLE,
                    Content = a.CONTENT,
                    Category = a.Category,
                    CategoryColor = a.CategoryColor,
                    Date = a.CREATED_AT
                })
                .ToListAsync();

            ViewBag.AllAnnouncements = allAnnouncements;

            // Fetch user's recent feedback submissions
            if (!string.IsNullOrEmpty(userId))
            {
                var recentFeedback = await _context.Feedbacks
                    .Where(f => f.UserId == int.Parse(userId))
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new FeedbackViewModel
                    {
                        FeedbackId = f.FeedbackId,
                        UserId = f.UserId,
                        UserName = f.User != null ? f.User.FirstName + " " + f.User.LastName : "Unknown",
                        HouseNumber = f.User != null && f.User.Homeowner != null ? f.User.Homeowner.HouseNumber : string.Empty,
                        FeedbackType = f.FeedbackType,
                        Description = f.Description,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd") // Already formatted as a string
                    })
                    .Take(3)
                    .ToListAsync();

                ViewBag.RecentFeedback = recentFeedback;
            }

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
            var user = _context.Users
                .Include(u => u.Homeowner) // Include the Homeowner navigation property
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.FirstName = $"{user.FirstName}";
            ViewBag.HouseNumber = user.HouseNumber ?? "N/A";
            ViewBag.CurrentBalance = "5,250.00";
            ViewBag.DueDate = "March 15, 2025";
            ViewBag.LastPayment = "2,500.00";
            ViewBag.LastPaymentDate = "February 15, 2025";
            
            // Add the ViewBag properties that are needed by the view
            ViewBag.TotalDue = 5250.00m;
            ViewBag.PaidThisMonth = 2500.00m;
            ViewBag.DueThisMonth = 3750.00m;
            ViewBag.OverdueAmount = 3000.00m;
            
            // Initialize ViewBag.UserBills to prevent NullReferenceException in the view
            ViewBag.UserBills = _context.UserBills
                .Include(b => b.BillingItem)
                .Where(b => b.UserId == user.UserId)
                .ToList();
            
            // Initialize ViewBag.Payments for the payment history section
            ViewBag.Payments = _context.Payments
                .Where(p => p.UserId == user.UserId)
                .OrderByDescending(p => p.PaymentDate)
                .Take(10)  // Limit to the most recent 10 payments
                .ToList();
                
            // Initialize ViewBag.PaymentMethods to prevent any potential NullReferenceException
            ViewBag.PaymentMethods = new List<dynamic>();
            
            // Added a UserProfileViewModel to match the model expected by the view
            var model = new UserProfileViewModel
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                HouseNumber = user.HouseNumber ?? string.Empty,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        public IActionResult Facilities()
        {
            var username = User.Identity?.Name;
            var user = _context.Users
                .Include(u => u.Homeowner) // Include the Homeowner navigation property
                .FirstOrDefault(u => u.Username == username);

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
            var user = _context.Users
                .Include(u => u.Homeowner) // Include the Homeowner navigation property
                .FirstOrDefault(u => u.Username == username);

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
            var user = _context.Users
                .Include(u => u.Homeowner) // Include the Homeowner navigation property
                .FirstOrDefault(u => u.Username == username);

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
            var user = _context.Users
                .Include(u => u.Homeowner) // Include the Homeowner navigation property
                .FirstOrDefault(u => u.Username == username);

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
            var user = _context.Users
                .Include(u => u.Homeowner) // Include the Homeowner navigation property
                .FirstOrDefault(u => u.Username == username);

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
                var existingUserWithSameUsername = _context.Users.FirstOrDefault(u => u.Username == model.Username);
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
                var existingUserWithSameEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
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

            // Explicitly update the Homeowner.HouseNumber
            if (user.Homeowner != null)
            {
                user.Homeowner.HouseNumber = model.HouseNumber ?? string.Empty;
            }
            else
            {
                // If Homeowner relation doesn't exist yet (shouldn't happen but just in case), create it
                _logger.LogWarning("Homeowner relationship missing for user {Username}, creating new entry", user.Username);
                user.Homeowner = new Homeowner
                {
                    UserId = user.UserId,
                    HouseNumber = model.HouseNumber ?? string.Empty,
                    User = user
                };
            }

            try
            {
                _context.SaveChanges();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(string feedbackType, string description)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction("Dashboard");
                }

                // Basic validation
                if (string.IsNullOrEmpty(feedbackType) || string.IsNullOrEmpty(description))
                {
                    TempData["ErrorMessage"] = "Feedback type and description are required.";
                    return RedirectToAction("Dashboard");
                }

                // Create new feedback entry
                var feedback = new Feedback
                {
                    UserId = int.Parse(userId),
                    FeedbackType = feedbackType,
                    Description = description,
                    Status = "Submitted",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Add to database
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // Success message
                TempData["SuccessMessage"] = "Your feedback has been submitted successfully.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback");
                TempData["ErrorMessage"] = "There was an error submitting your feedback. Please try again.";
                return RedirectToAction("Dashboard");
            }
        }
    }
}