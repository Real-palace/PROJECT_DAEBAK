using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using daebak_subdivision_website.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

namespace daebak_subdivision_website.Controllers
{
    [Authorize] // Ensures only authenticated users can access this controller
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(ApplicationDbContext context, ILogger<UserManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ✅ GET: Users List
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();

            // Convert List<User> to List<UserViewModel>
            var userViewModels = users.Select(u => new UserViewModel
            {
                Id = u.UserId, // Changed from UserId to Id
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                HouseNumber = u.HouseNumber,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return View("~/Views/Management/UserList.cshtml", userViewModels);
        }

        // ✅ GET: Add User Form
        public IActionResult AddUser()
        {
            return View("~/Views/Management/AddUser.cshtml"); // Redirect to AddUser.cshtml
        }

        // ✅ POST: Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Role = model.Role,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    HouseNumber = model.HouseNumber,
                    PasswordHash = HashPassword(model.Password), // Hash password using bcrypt
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {Username} created successfully", user.Username);

                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Management/AddUser.cshtml", model); // Ensure it stays on AddUser.cshtml if invalid
        }

        // ✅ GET: Edit User
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                HouseNumber = user.HouseNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View("~/Views/Management/EditUser.cshtml", model);
        }

        // ✅ POST: Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Username = model.Username;
                user.Email = model.Email;
                user.Role = model.Role;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.HouseNumber = model.HouseNumber;
                user.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = HashPassword(model.Password); // Hash new password using bcrypt
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Username} updated successfully", model.Username);
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Management/EditUser.cshtml", model);
        }

        // ✅ GET: Delete User (Show confirmation)
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Return confirmation page with user details
            var model = new UserViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                HouseNumber = user.HouseNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View("~/Views/Management/DeleteUser.cshtml", model);
        }

        // ✅ POST: Confirm Delete User
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} deleted successfully", user.Username);

            return RedirectToAction(nameof(Index));
        }

        // ✅ GET: User Details
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                HouseNumber = user.HouseNumber,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View("~/Views/Management/UserDetails.cshtml", model);
        }

        // ✅ Check if user exists
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        // ✅ Hash Password using bcrypt
        private string HashPassword(string password)
        {
            // Hash the password with bcrypt
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // ✅ Verify Password using bcrypt
        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
