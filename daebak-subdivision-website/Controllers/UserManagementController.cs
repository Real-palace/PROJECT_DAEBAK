using Microsoft.AspNetCore.Mvc;
using daebak_subdivision_website.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DaebakRealEstate.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, string role)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u =>
                    (u.FirstName + " " + u.LastName).Contains(search) ||
                    u.Email.Contains(search) ||
                    u.HouseNumber.Contains(search));
            }

            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                users = users.Where(u => u.Role == role);
            }

            // Convert List<User> to List<UserViewModel>
            var userViewModels = users.Select(u => new UserViewModel
            {
                Id = u.UserId,
                Username = u.Username,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                PhoneNumber = u.PhoneNumber,
                HouseNumber = u.HouseNumber
            }).ToList();

            return View("~/Views/Management/UserList.cshtml", userViewModels);
        }

        public IActionResult AddUser()
        {
            return View("~/Views/Management/AddUser.cshtml");
        }

        [HttpPost]
        public IActionResult AddUser(UserViewModel newUser)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = newUser.Username,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Email = newUser.Email,
                    PasswordHash = HashPassword(newUser.Password),
                    Role = newUser.Role,
                    PhoneNumber = newUser.PhoneNumber,
                    HouseNumber = newUser.HouseNumber
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/Management/AddUser.cshtml", newUser);
        }

        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            var viewModel = new UserViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                HouseNumber = user.HouseNumber
            };

            return View("~/Views/Management/EditUser.cshtml", viewModel);
        }

        [HttpPost]
        public IActionResult EditUser(UserViewModel updatedUser)
        {
            var user = _context.Users.Find(updatedUser.Id);
            if (user == null) return NotFound();

            user.Username = updatedUser.Username;
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.HouseNumber = updatedUser.HouseNumber;

            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                user.PasswordHash = HashPassword(updatedUser.Password);
            }

            user.Role = updatedUser.Role;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
