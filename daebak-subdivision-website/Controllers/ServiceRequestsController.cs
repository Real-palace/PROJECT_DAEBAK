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
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(ApplicationDbContext context, ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [Authorize(Roles = "HOMEOWNER")]
        public IActionResult Index()
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
                    Location = x.sr_u.sr.Location,
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

            return View("~/Views/Management/ServiceRequests.cshtml", serviceRequests);
        }

        [Authorize(Roles = "HOMEOWNER")]
        public IActionResult TrackRequests()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userId, out int userIdInt))
            {
                var serviceRequests = _context.ServiceRequests
                    .Where(sr => sr.UserId == userIdInt)
                    .Select(sr => new ServiceRequestView
                    {
                        Id = sr.Id,
                        RequestType = sr.RequestType,
                        Description = sr.Description,
                        Status = sr.Status,
                        CreatedAt = sr.CreatedAt
                    })
                    .ToList();

                return View(serviceRequests);
            }

            return View(new List<ServiceRequestView>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ServiceRequest request, IFormFile[] Images)
        {
            try
            {
                _logger.LogInformation("Attempting to create service request");
                  if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid model state: {Errors}", string.Join(", ", errors));
                    return Json(new { success = false, message = "Invalid request data", errors = errors });
                }

                var userId = User.FindFirst("UserId")?.Value;
                _logger.LogInformation("User ID from claims: {UserId}", userId);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User not authenticated - no user ID found in claims");
                    return Json(new { success = false, message = "User not authenticated" });
                }                if (!int.TryParse(userId, out int userIdInt))
                {
                    _logger.LogWarning("Invalid user ID format: {UserId}", userId);
                    return Json(new { success = false, message = "Invalid user ID format" });
                }

                var homeowner = await _context.Homeowners.FirstOrDefaultAsync(h => h.UserId == userIdInt);
                if (homeowner == null)
                {
                    _logger.LogWarning("Homeowner record not found for user ID: {UserId}", userId);
                    return Json(new { success = false, message = "User not found" });
                }                request.UserId = userIdInt;
                request.Status = "Open";
                request.CreatedAt = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
                request.StaffNotes = string.Empty;

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
                        innerException = ex.InnerException?.Message
                    }
                });
            }
        }

        [Authorize(Roles = "HOMEOWNER")]
        [HttpPost]
        [Route("Services/SubmitRequest")]
        public IActionResult SubmitRequest([FromForm] ServiceRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current user's ID from the custom claim
            var userId = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userId, out int userIdInt))
            {
                _logger.LogWarning("Failed to get user ID from claims");
                return Unauthorized();
            }

            // Set the user ID and initial values
            request.UserId = userIdInt;
            request.Status = "Open";
            request.CreatedAt = DateTime.Now;
            request.UpdatedAt = DateTime.Now;
            request.StaffNotes = string.Empty;
            request.Location = request.Location ?? string.Empty;

            // Add the request to the database
            _context.ServiceRequests.Add(request);
            _context.SaveChanges();

            // Redirect to the Services view with the track tab parameter
            return RedirectToAction("Services", "Homeowner", new { tab = "track" });
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

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [Route("Admin/ServiceRequests/GetAll")]
        public IActionResult GetAllServiceRequests()
        {
            var serviceRequests = _context.ServiceRequests
                .Join(_context.Users,
                    sr => sr.UserId,
                    u => u.UserId,
                    (sr, u) => new ServiceRequestView
                    {
                        Id = sr.Id,
                        UserId = sr.UserId,
                        RequestType = sr.RequestType,
                        Description = sr.Description,
                        Location = sr.Location,
                        Status = sr.Status,
                        CreatedAt = sr.CreatedAt,
                        UpdatedAt = sr.UpdatedAt,
                        RequestedBy = u.FirstName + " " + u.LastName,
                        StaffNotes = sr.StaffNotes
                    })
                .OrderByDescending(sr => sr.CreatedAt)
                .ToList();

            return Json(serviceRequests);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [Route("Admin/ServiceRequests/GetStats")]
        public IActionResult GetServiceRequestStats()
        {
            var stats = new
            {
                Total = _context.ServiceRequests.Count(),
                Pending = _context.ServiceRequests.Count(sr => sr.Status == "Open"),
                Completed = _context.ServiceRequests.Count(sr => sr.Status == "Completed"),
                Canceled = _context.ServiceRequests.Count(sr => sr.Status == "Rejected")
            };

            return Json(stats);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [Route("Admin/ServiceRequests/UpdateStatus")]
        public IActionResult UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (request == null)
                {
                    _logger.LogWarning("UpdateStatus called with null request");
                    return Json(new { success = false, message = "Invalid request data" });
                }

                _logger.LogInformation("Attempting to update service request {Id} with status {Status}", 
                    request.Id, request.Status);

                var serviceRequest = _context.ServiceRequests.Find(request.Id);
                if (serviceRequest == null)
                {
                    _logger.LogWarning("Service request not found with ID: {Id}", request.Id);
                    return Json(new { success = false, message = "Service request not found" });
                }

                // Validate status
                var validStatuses = new[] { "Open", "In Progress", "Scheduled", "Completed", "Rejected" };
                if (!validStatuses.Contains(request.Status))
                {
                    _logger.LogWarning("Invalid status provided: {Status}", request.Status);
                    return Json(new { success = false, message = "Invalid status provided" });
                }

                serviceRequest.Status = request.Status;
                serviceRequest.StaffNotes = request.StaffNotes;
                serviceRequest.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Saving changes to database for request {Id}", request.Id);
                _context.SaveChanges();
                
                _logger.LogInformation("Service request {Id} updated successfully", request.Id);
                return Json(new { success = true, message = "Service request updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service request {Id}. Error: {ErrorMessage}", 
                    request?.Id, ex.Message);
                return Json(new { 
                    success = false, 
                    message = "An error occurred while updating the service request",
                    error = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        public class UpdateStatusRequest
        {
            public int Id { get; set; }
            public string Status { get; set; }
            public string StaffNotes { get; set; }
        }

        [HttpDelete]
        [Authorize(Roles = "ADMIN")]
        [Route("Admin/ServiceRequests/Delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var serviceRequest = _context.ServiceRequests.Find(id);
                if (serviceRequest == null)
                {
                    return Json(new { success = false, message = "Service request not found" });
                }

                _context.ServiceRequests.Remove(serviceRequest);
                _context.SaveChanges();
                return Json(new { success = true, message = "Service request deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service request {Id}", id);
                return Json(new { success = false, message = "An error occurred while deleting the service request" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "HOMEOWNER")]
        [Route("Services/GetUserRequests")]
        public IActionResult GetUserRequests()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userId, out int userIdInt))
            {
                return Unauthorized();
            }

            var serviceRequests = _context.ServiceRequests
                .Where(sr => sr.UserId == userIdInt)
                .OrderByDescending(sr => sr.CreatedAt)
                .Select(sr => new ServiceRequestView
                {
                    Id = sr.Id,
                    RequestType = sr.RequestType,
                    Description = sr.Description,
                    Location = sr.Location,
                    Status = sr.Status,
                    CreatedAt = sr.CreatedAt,
                    UpdatedAt = sr.UpdatedAt
                })
                .ToList();

            return Json(serviceRequests);
        }

        [HttpGet]
        [Authorize(Roles = "HOMEOWNER,ADMIN")]
        [Route("ServiceRequests/GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int userIdInt))
                {
                    return Unauthorized();
                }

                // Admins can see all requests, homeowners can only see their own
                var isAdmin = User.IsInRole("ADMIN");
                var serviceRequest = await _context.ServiceRequests
                    .FirstOrDefaultAsync(r => r.Id == id && (isAdmin || r.UserId == userIdInt));

                if (serviceRequest == null)
                {
                    return NotFound(new { success = false, message = "Service request not found" });
                }

                return Json(new ServiceRequestView
                {
                    Id = serviceRequest.Id,
                    UserId = serviceRequest.UserId,
                    RequestType = serviceRequest.RequestType,
                    Description = serviceRequest.Description,
                    Location = serviceRequest.Location,
                    Status = serviceRequest.Status,
                    CreatedAt = serviceRequest.CreatedAt,
                    UpdatedAt = serviceRequest.UpdatedAt,
                    StaffNotes = serviceRequest.StaffNotes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service request details for ID: {Id}", id);
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving the service request details" });
            }
        }
    }
}
