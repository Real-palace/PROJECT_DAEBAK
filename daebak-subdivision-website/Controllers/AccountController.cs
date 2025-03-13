using daebak_subdivision_website.Models;
using Microsoft.AspNetCore.Mvc;

namespace daebak_subdivision_website.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Index.cshtml", model);
            }

            // Here you would add your authentication logic
            // For example, checking credentials against a database
            
            // For testing, we'll just redirect to a dashboard or return with an error
            if (model.Username == "admin" && model.Password == "password")
            {
                return RedirectToAction("Dashboard", "Home");
            }
            
            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View("~/Views/Home/Index.cshtml", model);
        }

        public IActionResult ForgotPassword()
        {
            // Implement forgot password functionality
            return View();
        }

        public IActionResult Contact()
        {
            // Implement contact functionality
            return View();
        }
    }
}
