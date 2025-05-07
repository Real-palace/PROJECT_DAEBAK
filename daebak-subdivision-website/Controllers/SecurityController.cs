using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace daebak_subdivision_website.Controllers
{
    public class SecurityController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SecurityController> _logger;

        public SecurityController(ApplicationDbContext dbContext, ILogger<SecurityController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // Homeowner security page
        [Authorize(Roles = "HOMEOWNER")]
        public IActionResult Index()
        {
            _logger.LogInformation("Homeowner security page accessed");
            return View("~/Views/Homeowner/Security.cshtml");
        }

        // Admin security page
        [Authorize(Roles = "ADMIN")]
        [Route("Admin/Security")]
        public IActionResult Admin()
        {
            _logger.LogInformation("Admin security page accessed");
            return View("~/Views/Admin/Security.cshtml");
        }

        // Request visitor pass (for homeowners)
        [HttpPost]
        [Authorize(Roles = "HOMEOWNER")]
        public async Task<IActionResult> RequestVisitorPass(VisitorPass visitorPass)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user's ID
                    var username = User.Identity?.Name;
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                    if (user == null)
                    {
                        return Unauthorized();
                    }

                    // Set the requesting user ID
                    visitorPass.RequestedById = user.UserId;
                    visitorPass.Status = "Pending"; // Initial status
                    visitorPass.CreatedAt = DateTime.Now;
                    visitorPass.UpdatedAt = DateTime.Now;

                    _dbContext.VisitorPasses.Add(visitorPass);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Visitor pass requested by {Username}", username);

                    TempData["SuccessMessage"] = "Visitor pass request submitted successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred when requesting visitor pass");
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }
            }

            return View("~/Views/Homeowner/Security.cshtml", visitorPass);
        }

        // Register vehicle (for homeowners)
        [HttpPost]
        [Authorize(Roles = "HOMEOWNER")]
        public async Task<IActionResult> RegisterVehicle(VehicleRegistration vehicle)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user's ID
                    var username = User.Identity?.Name;
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                    if (user == null)
                    {
                        return Unauthorized();
                    }

                    // Set the owner ID
                    vehicle.OwnerId = user.UserId;
                    vehicle.Status = "Active"; // Initial status
                    vehicle.RegistrationDate = DateTime.Now;
                    vehicle.CreatedAt = DateTime.Now;
                    vehicle.UpdatedAt = DateTime.Now;

                    _dbContext.VehicleRegistrations.Add(vehicle);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Vehicle registered by {Username}", username);

                    TempData["SuccessMessage"] = "Vehicle registered successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred when registering vehicle");
                    ModelState.AddModelError("", "An error occurred while processing your request.");
                }
            }

            return View("~/Views/Homeowner/Security.cshtml", vehicle);
        }

        // Get visitor passes for current homeowner
        [Authorize(Roles = "HOMEOWNER")]
        public async Task<IActionResult> GetHomeownerVisitorPasses()
        {
            var username = User.Identity?.Name;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Unauthorized();
            }

            var passes = await _dbContext.VisitorPasses
                .Where(p => p.RequestedById == user.UserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Json(passes);
        }

        // Get vehicles for current homeowner
        [Authorize(Roles = "HOMEOWNER")]
        public async Task<IActionResult> GetHomeownerVehicles()
        {
            var username = User.Identity?.Name;
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return Unauthorized();
            }

            var vehicles = await _dbContext.VehicleRegistrations
                .Where(v => v.OwnerId == user.UserId)
                .OrderByDescending(v => v.RegistrationDate)
                .ToListAsync();

            return Json(vehicles);
        }
        
        // Get emergency contacts
        public async Task<IActionResult> GetEmergencyContacts()
        {
            var contacts = await _dbContext.EmergencyContacts
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
                
            return Json(contacts);
        }

        // Admin: Get all visitor passes
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllVisitorPasses(string status = null, DateTime? date = null)
        {
            var query = _dbContext.VisitorPasses
                .Include(p => p.RequestedBy)
                .Include(p => p.RequestedBy.Homeowner)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (date.HasValue)
            {
                DateTime startOfDay = date.Value.Date;
                DateTime endOfDay = startOfDay.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.VisitDate >= startOfDay && p.VisitDate <= endOfDay);
            }

            var passes = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.PassId,
                    p.VisitorName,
                    p.VisitorPhone,
                    Homeowner = p.RequestedBy.FirstName + " " + p.RequestedBy.LastName,
                    HomeownerId = p.RequestedById,
                    HomeownerAddress = p.RequestedBy.Homeowner != null ? p.RequestedBy.Homeowner.HouseNumber : "N/A",
                    p.VisitDate,
                    p.Purpose,
                    p.VehiclePlate,
                    p.Status,
                    p.Notes,
                    p.CreatedAt
                })
                .ToListAsync();

            return Json(passes);
        }

        // Admin: Get all vehicles
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllVehicles(string status = null, string type = null)
        {
            var query = _dbContext.VehicleRegistrations
                .Include(v => v.Owner)
                .Include(v => v.Owner.Homeowner)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(v => v.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(v => v.VehicleType == type);
            }

            var vehicles = await query
                .OrderByDescending(v => v.RegistrationDate)
                .Select(v => new
                {
                    v.RegistrationId,
                    v.PlateNumber,
                    v.VehicleMake,
                    v.VehicleModel,
                    v.VehicleColor,
                    v.VehicleYear,
                    v.VehicleType,
                    Owner = v.Owner.FirstName + " " + v.Owner.LastName,
                    OwnerId = v.OwnerId,
                    OwnerAddress = v.Owner.Homeowner != null ? v.Owner.Homeowner.HouseNumber : "N/A",
                    v.Status,
                    v.RegistrationDate
                })
                .ToListAsync();

            return Json(vehicles);
        }

        // Admin: Approve visitor pass
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ApproveVisitorPass(int passId)
        {
            var pass = await _dbContext.VisitorPasses.FindAsync(passId);
            if (pass == null)
            {
                return NotFound();
            }

            pass.Status = "Approved";
            pass.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Visitor pass {PassId} approved by {Username}", passId, User.Identity?.Name);
            return Json(new { success = true });
        }

        // Admin: Reject visitor pass
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RejectVisitorPass(int passId, string reason = null)
        {
            var pass = await _dbContext.VisitorPasses.FindAsync(passId);
            if (pass == null)
            {
                return NotFound();
            }

            pass.Status = "Rejected";
            pass.Notes = string.IsNullOrEmpty(pass.Notes) 
                ? $"Rejection reason: {reason}" 
                : $"{pass.Notes}\nRejection reason: {reason}";
            pass.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Visitor pass {PassId} rejected by {Username}", passId, User.Identity?.Name);
            return Json(new { success = true });
        }

        // Admin: Mark vehicle as inactive
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeactivateVehicle(int registrationId)
        {
            var vehicle = await _dbContext.VehicleRegistrations.FindAsync(registrationId);
            if (vehicle == null)
            {
                return NotFound();
            }

            vehicle.Status = "Inactive";
            vehicle.UpdatedAt = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Vehicle {RegistrationId} deactivated by {Username}", registrationId, User.Identity?.Name);
            return Json(new { success = true });
        }

        // Admin: Create security alert
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateSecurityAlert(SecurityAlert alert)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current admin's ID
                    var username = User.Identity?.Name;
                    var admin = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                    if (admin == null)
                    {
                        return Unauthorized();
                    }

                    alert.CreatedById = admin.UserId;
                    alert.CreatedAt = DateTime.Now;
                    alert.UpdatedAt = DateTime.Now;

                    _dbContext.SecurityAlerts.Add(alert);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Security alert created by {Username}", username);

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred when creating security alert");
                    return Json(new { success = false, message = "An error occurred while creating the alert." });
                }
            }

            return Json(new { success = false, message = "Invalid data" });
        }

        // Get all security alerts (both admin and homeowner can access)
        public async Task<IActionResult> GetSecurityAlerts()
        {
            var alerts = await _dbContext.SecurityAlerts
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.AlertId,
                    a.Title,
                    a.Content,
                    a.AlertType,
                    a.ExpiryDate,
                    CreatedBy = a.CreatedBy.FirstName + " " + a.CreatedBy.LastName,
                    a.CreatedAt
                })
                .ToListAsync();

            return Json(alerts);
        }
    }
}