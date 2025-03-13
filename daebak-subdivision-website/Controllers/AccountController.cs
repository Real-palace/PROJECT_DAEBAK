using daebak_subdivision_website.Models;
using Microsoft.AspNetCore.Mvc;

namespace daebak_subdivision_website.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Temporary static account for testing
            const string tempUsername = "homeowner";
            const string tempPassword = "password123";

            if (ModelState.IsValid)
            {
                // Check if credentials match our temporary account
                if (model.Username == tempUsername && model.Password == tempPassword)
                {
                    // Successful login - redirect to home page (fixed to redirect to Home action)
                    return RedirectToAction("Home", "Home");
                }

                // Invalid login attempt
                ModelState.AddModelError(string.Empty, "Invalid username or password");
            }

            // If we got this far, something failed, redisplay form
            return View("~/Views/Home/Index.cshtml", model);
        }

        public IActionResult ForgotPassword()
        {
            // This is just a placeholder for now
            return View();
        }

        public IActionResult Contact()
        {
            // This is just a placeholder for now
            return View();
        }

        public IActionResult Logout()
        {
            // Here you would implement logout functionality
            // For now, just redirect to the login page
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile()
        {
            // In a real application, we would fetch this data from the database
            // For now, create mock data based on our database schema
            var userProfile = new UserProfileViewModel
            {
                Username = "homeowner",
                FirstName = "John",
                LastName = "Doe",
                Email = "homeowner@example.com",
                PhoneNumber = "09053501976",
                HouseNumber = "Lot 45",
                ProfilePicture = "/images/profile/default.jpg",
                Role = "Homeowner",
                CreatedAt = new DateTime(2020, 1, 15)
            };

            ViewBag.UserName = $"{userProfile.FirstName} {userProfile.LastName}";
            ViewBag.MemberSince = userProfile.CreatedAt.ToString("MMMM d, yyyy");

            return View(userProfile);
        }

        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            // Remove password validation if not changing password
            if (string.IsNullOrEmpty(model.CurrentPassword) && string.IsNullOrEmpty(model.NewPassword) && 
                string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                // If all password fields are empty, password is not being changed
                ModelState.Remove("CurrentPassword");
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmNewPassword");
            }
            else if (!string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                // If user is trying to set a new password, current password is required
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to set a new password");
                }
                
                // Check if new password meets requirements (only if trying to change password)
                if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword.Length < 8)
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 8 characters");
                }
            }

            if (ModelState.IsValid)
            {
                // In a real application, update the user profile in the database
                // For this example, we'll simulate success and show what would be updated

                // Create a list of changed fields to display in the success message
                List<string> changedFields = new List<string>();
                changedFields.Add("profile"); // Basic profile is always considered updated
                
                // Check if password is being changed
                if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.CurrentPassword))
                {
                    // In real app we'd verify current password and hash new password
                    changedFields.Add("password");
                }
                
                // Check if email was changed
                if (!string.IsNullOrEmpty(model.Email) && model.Email != "homeowner@example.com") // Compare with original
                {
                    changedFields.Add("email");
                }

                string updateMessage = "Profile updated successfully!";
                if (changedFields.Count > 1)
                {
                    updateMessage = $"{string.Join(", ", changedFields.Take(changedFields.Count - 1))} and {changedFields.Last()} updated successfully!";
                }
                
                TempData["SuccessMessage"] = updateMessage;
                return RedirectToAction("Profile");
            }

            // If we got here, something went wrong - redisplay the form
            ViewBag.UserName = $"{model.FirstName} {model.LastName}";
            ViewBag.MemberSince = model.CreatedAt.ToString("MMMM d, yyyy") ?? "Unknown";
            return View("Profile", model);
        }
    }
}