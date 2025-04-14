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

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Username == model.Username);
                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role.ToUpper()),
                        new Claim("UserId", user.UserId.ToString())
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Redirect based on role
                    switch (user.Role.ToUpper())
                    {
                        case "ADMIN":
                            return RedirectToAction("AdminPage", "Account");
                        case "HOMEOWNER":
                            return RedirectToAction("HomeOwner", "Account");
                        case "STAFF":
                            return RedirectToAction("StaffPage", "Account");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            return View(model);
        }

        public IActionResult AdminPage()
        {
            return View("Admin");
        }

        public IActionResult HomeOwner()
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning($"DEBUG: HomeOwner access failed. User '{username}' not found.");
                return RedirectToAction("Index", "Home");
            }

            // Case-insensitive role check
            if (!string.Equals(user.Role, "HOMEOWNER", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"DEBUG: Access denied. User '{username}' is not a homeowner.");
                return RedirectToAction("Index", "Home");
            }

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

            _logger.LogInformation($"DEBUG: HomeOwner profile loaded for user {model.Username} with house number {model.HouseNumber}");

            return View(model);
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


        [HttpPost]
        public IActionResult UpdateProfile(UserProfileViewModel model)
        {
            var username = User.Identity?.Name;
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                _logger.LogWarning($"DEBUG: UpdateProfile failed. User '{username}' not found.");
                return RedirectToAction("Index", "Home");
            }

            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.ProfilePicture = model.ProfilePicture;
            user.UpdatedAt = System.DateTime.Now;

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (VerifyPassword(model.CurrentPassword, user.PasswordHash))
                {
                    user.PasswordHash = HashPassword(model.NewPassword);
                    _logger.LogInformation("DEBUG: Password updated successfully.");
                }
                else
                {
                    _logger.LogWarning("DEBUG: Incorrect current password during profile update.");
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View("Profile", model);
                }
            }

            _dbContext.SaveChanges();
            TempData["SuccessMessage"] = "Profile updated successfully!";
            _logger.LogInformation($"DEBUG: Profile updated for user {user.Username}");

            return RedirectToAction("Profile");
        }

        // FIXED: Use BCrypt for password hashing
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // FIXED: Use BCrypt for password verification
        private static bool VerifyPassword(string inputPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }
    }
}
