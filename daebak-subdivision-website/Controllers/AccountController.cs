using Microsoft.AspNetCore.Mvc;
using daebak_subdivision_website.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace daebak_subdivision_website.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Looks for Views/Account/Login.cshtml
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Temporary static accounts for demonstration
            const string adminUsername = "admin";
            const string adminPassword = "password";

            const string homeownerUsername = "homeowner";
            const string homeownerPassword = "password123";

            if (ModelState.IsValid)
            {
                if (model.Username == adminUsername && model.Password == adminPassword)
                {
                    return RedirectToAction("SecondPage");
                }
                else if (model.Username == homeownerUsername && model.Password == homeownerPassword)
                {
                    return RedirectToAction("Home", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        public IActionResult SecondPage()
        {
            var model = new SecondPageModel(); // Ensure the model exists
            return View("2nd", model); // Ensure the file exists: Views/Account/2nd.cshtml
        }

        public IActionResult ForgotPassword()
        {
            // Placeholder for forgot password functionality
            return View();
        }

        public IActionResult Contact()
        {
            // Placeholder for contact functionality
            return View();
        }

        public IActionResult Logout()
        {
            // Clears session or authentication data if implemented
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Profile()
        {
            // Mock user profile data
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
                ModelState.Remove("CurrentPassword");
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmNewPassword");
            }
            else if (!string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to set a new password");
                }

                if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword.Length < 8)
                {
                    ModelState.AddModelError("NewPassword", "Password must be at least 8 characters");
                }
            }

            if (ModelState.IsValid)
            {
                List<string> changedFields = new List<string> { "profile" };

                if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.CurrentPassword))
                {
                    changedFields.Add("password");
                }

                if (!string.IsNullOrEmpty(model.Email) && model.Email != "homeowner@example.com")
                {
                    changedFields.Add("email");
                }

                TempData["SuccessMessage"] = $"{string.Join(", ", changedFields.Take(changedFields.Count - 1))} and {changedFields.Last()} updated successfully!";
                return RedirectToAction("Profile");
            }

            ViewBag.UserName = $"{model.FirstName} {model.LastName}";
            ViewBag.MemberSince = model.CreatedAt.ToString("MMMM d, yyyy") ?? "Unknown";
            return View("Profile", model);
        }
    }
}
