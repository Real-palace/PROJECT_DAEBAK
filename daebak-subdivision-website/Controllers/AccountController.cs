using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using daebak_subdivision_website.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;

namespace daebak_subdivision_website.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext dbContext, ILogger<AccountController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View("~/Views/Login.cshtml", new LoginViewModel());
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("~/Views/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Clear any existing errors
            ModelState.Clear();

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", "Username is required");
            }

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                return View("~/Views/Login.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Username == model.Username);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: Username '{model.Username}' not found");
                    ModelState.AddModelError(string.Empty, "Username doesn't exist");
                    return View("~/Views/Login.cshtml", model);
                }

                if (!VerifyPassword(model.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed: Invalid password for user '{model.Username}'");
                    ModelState.AddModelError(string.Empty, "Incorrect password");
                    return View("~/Views/Login.cshtml", model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role.ToUpper()),
                    new Claim("UserId", user.UserId.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                _logger.LogInformation($"User '{user.Username}' logged in successfully");

                // Redirect based on role
                switch (user.Role.ToUpper())
                {
                    case "ADMIN":
                        return RedirectToAction("Dashboard", "Admin");
                    case "HOMEOWNER":
                        return RedirectToAction("Dashboard", "Homeowner");
                    case "STAFF":
                        return RedirectToAction("Dashboard", "Staff");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }

            // If we got this far, something failed
            return View("~/Views/Login.cshtml", model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("DEBUG: User logged out.");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning($"DEBUG: Profile access failed. User '{username}' not found.");
                return RedirectToAction("Index", "Home");
            }

            var homeowner = _dbContext.Homeowners.FirstOrDefault(h => h.UserId == user.UserId);
            if (homeowner == null)
            {
                _logger.LogWarning($"DEBUG: Profile access denied. User '{username}' is not a homeowner.");
                return RedirectToAction("Index", "Home");
            }

            var userProfile = new UserProfileViewModel
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                HouseNumber = homeowner.HouseNumber ?? string.Empty,
                ProfilePicture = user.ProfilePicture ?? "/images/profile/default.jpg",
                Role = "Homeowner",
                CreatedAt = user.CreatedAt
            };

            ViewBag.UserName = $"{userProfile.FirstName} {userProfile.LastName}";
            ViewBag.MemberSince = userProfile.CreatedAt.ToString("MMMM d, yyyy");

            _logger.LogInformation($"DEBUG: Profile loaded for homeowner {userProfile.Username}");

            return View(userProfile);
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string inputPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }
    }
}
