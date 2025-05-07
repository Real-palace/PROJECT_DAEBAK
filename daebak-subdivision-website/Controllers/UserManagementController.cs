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
    [Authorize(Roles = "ADMIN")]
    [Route("[controller]")]
    public class UserManagementController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(ApplicationDbContext dbContext, ILogger<UserManagementController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // GET: UserManagement
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var users = await _dbContext.Users
                .Include(u => u.Homeowner)
                .Include(u => u.Staff)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Create an AdminPageModel to satisfy the layout's requirements
            var viewModel = new AdminPageModel
            {
                // Initialize any properties required by the layout
            };

            // Pass the users as ViewData so the view can still access them
            ViewData["Users"] = users;

            return View(viewModel);
        }

        // GET: UserManagement/Create
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            // Create an AdminPageModel to satisfy the layout's requirements
            var adminPageModel = new AdminPageModel();

            // Pass the UserViewModel as ViewData so the view can still access it
            ViewData["UserViewModel"] = new UserViewModel();

            return View(adminPageModel);
        }

        // POST: UserManagement/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            // Add custom conditional validation for Department
            if (model.Role?.ToUpper() != "STAFF")
            {
                // If not staff, remove any Department validation errors
                ModelState.Remove("Department");
            }
            else if (string.IsNullOrEmpty(model.Department) && model.Role?.ToUpper() == "STAFF")
            {
                // If staff but no department is selected, add custom error
                ModelState.AddModelError("Department", "The Department field is required for Staff.");
            }

            // Add custom conditional validation for HouseNumber
            if (model.Role?.ToUpper() != "HOMEOWNER")
            {
                // If not homeowner, remove any HouseNumber validation errors
                ModelState.Remove("HouseNumber");
            }
            else if (string.IsNullOrEmpty(model.HouseNumber) && model.Role?.ToUpper() == "HOMEOWNER")
            {
                // If homeowner but no house number is entered, add custom error
                ModelState.AddModelError("HouseNumber", "The House Number field is required for Homeowners.");
            }

            if (ModelState.IsValid)
            {
                // Check for existing username or email
                if (await _dbContext.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    var adminPageModel = new AdminPageModel();
                    ViewData["UserViewModel"] = model;
                    return View(adminPageModel);
                }

                if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered");
                    var adminPageModel = new AdminPageModel();
                    ViewData["UserViewModel"] = model;
                    return View(adminPageModel);
                }

                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                // If the user is a homeowner, create homeowner record
                if (model.Role.ToUpper() == "HOMEOWNER" && !string.IsNullOrEmpty(model.HouseNumber))
                {
                    var homeowner = new Homeowner
                    {
                        UserId = user.UserId,
                        HouseNumber = model.HouseNumber
                    };

                    _dbContext.Homeowners.Add(homeowner);
                    await _dbContext.SaveChangesAsync();
                }
                // If the user is staff, create staff record
                else if (model.Role.ToUpper() == "STAFF" && !string.IsNullOrEmpty(model.Department))
                {
                    var staff = new Staff
                    {
                        UserId = user.UserId,
                        Department = model.Department
                    };

                    _dbContext.Staff.Add(staff);
                    await _dbContext.SaveChangesAsync();
                }

                _logger.LogInformation($"User created: {user.Username} with role {user.Role}");
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got here, model validation failed - wrap in AdminPageModel
            var adminModel = new AdminPageModel();
            ViewData["UserViewModel"] = model;
            return View(adminModel);
        }

        // GET: UserManagement/Edit/5
        [HttpGet]
        [Route("Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _dbContext.Users
                .Include(u => u.Homeowner)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = new UserViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Role = user.Role,
                HouseNumber = user.Homeowner?.HouseNumber ?? string.Empty,
                Department = user.Staff?.Department ?? string.Empty,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            // Create an AdminPageModel to satisfy the layout's requirements
            var adminPageModel = new AdminPageModel();

            // Pass the UserViewModel as ViewData
            ViewData["UserViewModel"] = userViewModel;

            return View(adminPageModel);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserViewModel model)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            // Add custom conditional validation for Department
            if (model.Role?.ToUpper() != "STAFF")
            {
                // If not staff, remove any Department validation errors
                ModelState.Remove("Department");
            }
            else if (string.IsNullOrEmpty(model.Department) && model.Role?.ToUpper() == "STAFF")
            {
                // If staff but no department is selected, add custom error
                ModelState.AddModelError("Department", "The Department field is required for Staff.");
            }

            // Add custom conditional validation for HouseNumber
            if (model.Role?.ToUpper() != "HOMEOWNER")
            {
                // If not homeowner, remove any HouseNumber validation errors
                ModelState.Remove("HouseNumber");
            }
            else if (string.IsNullOrEmpty(model.HouseNumber) && model.Role?.ToUpper() == "HOMEOWNER")
            {
                // If homeowner but no house number is entered, add custom error
                ModelState.AddModelError("HouseNumber", "The House Number field is required for Homeowners.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _dbContext.Users
                        .Include(u => u.Homeowner)
                        .Include(u => u.Staff)
                        .FirstOrDefaultAsync(u => u.UserId == id);

                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Check username uniqueness (if changed)
                    if (user.Username != model.Username &&
                        await _dbContext.Users.AnyAsync(u => u.Username == model.Username && u.UserId != id))
                    {
                        ModelState.AddModelError("Username", "Username already exists");
                        var adminPageModel = new AdminPageModel();
                        ViewData["UserViewModel"] = model;
                        return View(adminPageModel);
                    }

                    // Check email uniqueness (if changed)
                    if (user.Email != model.Email &&
                        await _dbContext.Users.AnyAsync(u => u.Email == model.Email && u.UserId != id))
                    {
                        ModelState.AddModelError("Email", "Email is already registered");
                        var adminPageModel = new AdminPageModel();
                        ViewData["UserViewModel"] = model;
                        return View(adminPageModel);
                    }

                    // Update user properties
                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Role = model.Role;
                    user.UpdatedAt = DateTime.Now;

                    // Update password if provided
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    }

                    _dbContext.Update(user);

                    // Handle homeowner data
                    if (model.Role.ToUpper() == "HOMEOWNER")
                    {
                        var homeowner = await _dbContext.Homeowners.FirstOrDefaultAsync(h => h.UserId == user.UserId);
                        if (homeowner != null)
                        {
                            homeowner.HouseNumber = model.HouseNumber ?? string.Empty;
                            _dbContext.Update(homeowner);
                        }
                        else if (!string.IsNullOrEmpty(model.HouseNumber))
                        {
                            // Create new homeowner record if doesn't exist
                            _dbContext.Homeowners.Add(new Homeowner
                            {
                                UserId = user.UserId,
                                HouseNumber = model.HouseNumber
                            });
                        }

                        // Remove staff record if role changed
                        var staffToRemove = await _dbContext.Staff.FirstOrDefaultAsync(s => s.UserId == user.UserId);
                        if (staffToRemove != null)
                        {
                            _dbContext.Staff.Remove(staffToRemove);
                        }
                    }
                    // Handle staff data
                    else if (model.Role.ToUpper() == "STAFF")
                    {
                        var staff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.UserId == user.UserId);
                        if (staff != null)
                        {
                            staff.Department = model.Department ?? string.Empty;
                            _dbContext.Update(staff);
                        }
                        else if (!string.IsNullOrEmpty(model.Department))
                        {
                            // Create new staff record if doesn't exist
                            _dbContext.Staff.Add(new Staff
                            {
                                UserId = user.UserId,
                                Department = model.Department
                            });
                        }

                        // Remove homeowner record if role changed
                        var homeownerToRemove = await _dbContext.Homeowners.FirstOrDefaultAsync(h => h.UserId == user.UserId);
                        if (homeownerToRemove != null)
                        {
                            _dbContext.Homeowners.Remove(homeownerToRemove);
                        }
                    }
                    else
                    {
                        // If user is neither homeowner nor staff, remove both records if they exist
                        var homeownerToRemove = await _dbContext.Homeowners.FirstOrDefaultAsync(h => h.UserId == user.UserId);
                        if (homeownerToRemove != null)
                        {
                            _dbContext.Homeowners.Remove(homeownerToRemove);
                        }

                        var staffToRemove = await _dbContext.Staff.FirstOrDefaultAsync(s => s.UserId == user.UserId);
                        if (staffToRemove != null)
                        {
                            _dbContext.Staff.Remove(staffToRemove);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"User updated: {user.Username}");
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If we get here, something failed - wrap in AdminPageModel
            var adminPageModelForError = new AdminPageModel();
            ViewData["UserViewModel"] = model;
            return View(adminPageModelForError);
        }

        // GET: UserManagement/Delete/5
        [HttpGet]
        [Route("Delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _dbContext.Users
                .Include(u => u.Homeowner)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: UserManagement/Delete/5
        [HttpPost]
        [Route("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user != null)
            {
                // First delete the homeowner record if it exists
                var homeowner = await _dbContext.Homeowners.FirstOrDefaultAsync(h => h.UserId == id);
                if (homeowner != null)
                {
                    _dbContext.Homeowners.Remove(homeowner);
                }

                // Delete the staff record if it exists
                var staff = await _dbContext.Staff.FirstOrDefaultAsync(s => s.UserId == id);
                if (staff != null)
                {
                    _dbContext.Staff.Remove(staff);
                }

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"User deleted: {user.Username}");
                TempData["SuccessMessage"] = "User deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Change user status (active/inactive)
        [HttpPost]
        [Route("ToggleStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Add status toggle functionality here if you have a status field
            // For now just return success
            return Json(new { success = true });
        }

        // Helper method to check if user exists
        private bool UserExists(int id)
        {
            return _dbContext.Users.Any(e => e.UserId == id);
        }
    }
}
