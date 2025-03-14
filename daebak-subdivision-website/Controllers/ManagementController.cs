using Microsoft.AspNetCore.Mvc;
using daebak_subdivision_website.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace daebak_subdivision_website.Controllers
{
    public class ManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Management action to serve Management.cshtml
        public IActionResult Management()
        {
            return View(); // Ensure /Views/Management/Management.cshtml exists
        }

        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            var model = new UserViewModel
            {
                Id = user.UserId,
                Username = user.Username ?? "N/A",
                Email = user.Email ?? "N/A",
                FirstName = user.FirstName ?? "N/A",
                LastName = user.LastName ?? "N/A",
                PhoneNumber = user.PhoneNumber,
                HouseNumber = user.HouseNumber,
                Role = user.Role ?? "N/A"
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.Find(model.Id);
                if (user == null) return NotFound();

                user.Username = model.Username;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.HouseNumber = model.HouseNumber;
                user.Role = model.Role;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = HashPassword(model.Password);
                }

                _context.Users.Update(user);
                _context.SaveChanges();
                return RedirectToAction("Management");
            }

            return View(model);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
