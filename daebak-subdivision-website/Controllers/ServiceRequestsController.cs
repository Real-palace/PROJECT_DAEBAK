using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace daebak_subdivision_website.Controllers
{
public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(ApplicationDbContext context, ILogger<ServiceRequestsController> logger)
    {
        _context = context;
            _logger = logger;
    }

    public IActionResult Index()
    {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userId, out int userIdInt))
            {
                var serviceRequests = _context.ServiceRequests
                    .Include(sr => sr.Homeowner)
                    .Where(sr => sr.UserId == userIdInt)
                    .Select(sr => new ServiceRequestView
                    {
                        Id = sr.Id,
                        UserId = sr.UserId,
                        HouseNumber = sr.HouseNumber,
                        RequestType = sr.RequestType,
                        Description = sr.Description,
                        Status = sr.Status,
                        CreatedAt = sr.CreatedAt,
                        UpdatedAt = sr.UpdatedAt,
                        AssignedTo = sr.AssignedTo,
                        RequestedBy = _context.Users
                            .Where(u => u.UserId == sr.UserId)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault(),
                        AssignedToName = _context.Users
                            .Where(u => u.UserId == sr.AssignedTo)
                            .Select(u => u.FirstName + " " + u.LastName)
                            .FirstOrDefault() ?? "Unassigned",
                        Location = sr.Location,
                        AdminResponse = sr.StaffNotes,
                        StaffNotes = sr.StaffNotes,
                        ScheduledDate = sr.ScheduledDate
                    })
                    .OrderByDescending(sr => sr.CreatedAt)
            .ToList();

                return View("~/Views/Homeowner/Services.cshtml", serviceRequests);
            }

            return View("~/Views/Homeowner/Services.cshtml", new List<ServiceRequestView>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] ServiceRequest request, IFormFile[] Images)
        {
            try
            {
                _logger.LogInformation("Attempting to create service request");
                
                // Log all claims for debugging
                var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogInformation("User claims: {Claims}", string.Join(", ", claims));
                
                // Clear model state for navigation properties since they're not part of the form
                ModelState.Remove("User");
                ModelState.Remove("Homeowner");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid model state: {Errors}", string.Join(", ", errors));
                    return Json(new { 
                        success = false, 
                        message = "Invalid request data", 
                        errors = errors,
                        debugInfo = new {
                            modelState = ModelState.ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                            )
                        }
                    });
                }

                // Get user ID from claims - try multiple claim types
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                            User.FindFirstValue("UserId") ?? 
                            User.FindFirstValue("sub");
                            
                _logger.LogInformation("User ID from claims: {UserId}", userId);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated - no user ID found in claims");
                    return Json(new { 
                        success = false, 
                        message = "User not authenticated",
                        debugInfo = new {
                            isAuthenticated = User.Identity?.IsAuthenticated,
                            authenticationType = User.Identity?.AuthenticationType,
                            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                        }
                    });
                }

                if (!int.TryParse(userId, out int userIdInt))
                {
                    _logger.LogWarning("Invalid user ID format: {UserId}", userId);
                    return Json(new { success = false, message = "Invalid user ID format" });
                }

                // Get user from database
                var user = await _context.Users.FindAsync(userIdInt);
                if (user == null)
                {
                    _logger.LogWarning("User not found in database: {UserId}", userId);
                    return Json(new { success = false, message = "User not found" });
                }

                // Get homeowner record
                var homeowner = await _context.Homeowners.FirstOrDefaultAsync(h => h.UserId == userIdInt);
                if (homeowner == null)
                {
                    _logger.LogWarning("Homeowner record not found for user: {UserId}", userId);
                    return Json(new { success = false, message = "Homeowner record not found" });
                }

                // Set request properties
                request.UserId = userIdInt;
                request.Status = "Pending";
                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Creating service request for user {UserId}", userId);
                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                // Handle image uploads if any
                if (Images != null && Images.Length > 0)
                {
                    foreach (var image in Images)
                    {
                        if (image.Length > 0)
                        {
                            // Validate file type
                            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                            if (!allowedTypes.Contains(image.ContentType.ToLower()))
                            {
                                _logger.LogWarning("Invalid file type: {ContentType}", image.ContentType);
                                continue;
                            }

                            // Validate file size (10MB max)
                            if (image.Length > 10 * 1024 * 1024)
                            {
                                _logger.LogWarning("File too large: {Size} bytes", image.Length);
                                continue;
                            }

                            // Save the image and create a record in the database
                            var fileName = $"{request.Id}_{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                            var filePath = Path.Combine("uploads", "service-requests", fileName);
                            
                            // Ensure directory exists
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(stream);
                            }

                            // Add image record to database
                            var requestImage = new RequestImage
                            {
                                RequestId = request.Id,
                                ImagePath = filePath,
                                UploadedAt = DateTime.UtcNow
                            };
                            _context.RequestImages.Add(requestImage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Service request created successfully with ID: {RequestId}", request.Id);
                return Json(new { success = true, message = "Service request submitted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                return Json(new { 
                    success = false, 
                    message = "An error occurred while creating the service request",
                    debugInfo = new {
                        error = ex.Message,
                        stackTrace = ex.StackTrace,
                        innerException = ex.InnerException?.Message,
                        modelState = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        ),
                        claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    }
                });
            }
        }

        [Authorize]
    public IActionResult TrackRequests()
    {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userId, out int userIdInt))
        {
            var serviceRequests = _context.ServiceRequests
                    .Include(sr => sr.Homeowner)
                    .Where(sr => sr.UserId == userIdInt)
                .Select(sr => new ServiceRequestView
                {
                    Id = sr.Id,
                    RequestType = sr.RequestType,
                    Description = sr.Description,
                    Status = sr.Status,
                        CreatedAt = sr.CreatedAt,
                        UpdatedAt = sr.UpdatedAt,
                        AdminResponse = sr.StaffNotes,
                        StaffNotes = sr.StaffNotes,
                        HouseNumber = sr.HouseNumber,
                        ScheduledDate = sr.ScheduledDate
                    })
                    .OrderByDescending(sr => sr.CreatedAt)
                .ToList();

                return View(serviceRequests);
            }

            return View(new List<ServiceRequestView>());
    }

    // Add this action for admin service requests
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/ServiceRequests/List")]
    public IActionResult AdminServiceRequests()
    {
        var serviceRequests = _context.ServiceRequests
            .Join(_context.Users,
                sr => sr.UserId,
                u => u.UserId,
                (sr, u) => new { sr, RequestedBy = u.FirstName + " " + u.LastName })
            .GroupJoin(_context.Users,
                sr_u => sr_u.sr.AssignedTo,
                u => u.UserId,
                (sr_u, assigned) => new { sr_u, assigned })
            .SelectMany(x => x.assigned.DefaultIfEmpty(), (x, assigned) => new ServiceRequestView
            {
                Id = x.sr_u.sr.Id,
                UserId = x.sr_u.sr.UserId,
                HouseNumber = x.sr_u.sr.HouseNumber,
                RequestType = x.sr_u.sr.RequestType,
                Description = x.sr_u.sr.Description,
                Status = x.sr_u.sr.Status,
                CreatedAt = x.sr_u.sr.CreatedAt,
                UpdatedAt = x.sr_u.sr.UpdatedAt,
                AssignedTo = x.sr_u.sr.AssignedTo,
                RequestedBy = x.sr_u.RequestedBy,
                AssignedToName = assigned != null ? assigned.FirstName + " " + assigned.LastName : "Unassigned"
            })
            .ToList();

        return Json(serviceRequests);
    }

    // Add this action for the admin service requests page
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    [Route("Admin/ServiceRequests")]
    public IActionResult AdminServiceRequestsPage()
    {
        // This will render the Admin/ServiceRequests view
        return View("~/Views/Admin/ServiceRequests.cshtml");
    }

    // Add these API endpoints for admin operations
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/ServiceRequests/UpdateStatus")]
    public IActionResult UpdateServiceRequestStatus(int id, string status, string adminResponse)
    {
        var serviceRequest = _context.ServiceRequests.Find(id);
        if (serviceRequest == null)
        {
            return NotFound();
        }

        serviceRequest.Status = status;
        serviceRequest.AdminResponse = adminResponse;
        serviceRequest.UpdatedAt = System.DateTime.Now;
        
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpDelete]
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/ServiceRequests/Delete/{id}")]
    public IActionResult DeleteServiceRequest(int id)
    {
        var serviceRequest = _context.ServiceRequests.Find(id);
        if (serviceRequest == null)
        {
            return NotFound();
        }

        _context.ServiceRequests.Remove(serviceRequest);
        _context.SaveChanges();
        return Json(new { success = true });
        }
    }
}
