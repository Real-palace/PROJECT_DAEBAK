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
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext dbContext, ILogger<AdminController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            var events = await _dbContext.Events
                .Include(e => e.CreatedBy)
                .OrderBy(e => e.StartDate)  // Changed from EventDate to StartDate
                .ToListAsync();

            // Get counts for dashboard statistics
            int userCount = await _dbContext.Users.CountAsync();
            decimal paymentsReceived = await _dbContext.Payments.SumAsync(p => p.Amount);
            int reservationsCount = await _dbContext.FacilityReservations.CountAsync();
            int serviceRequestsCount = await _dbContext.ServiceRequests.CountAsync();

            var model = new AdminPageModel
            {
                Events = events,
                UserCount = userCount,
                PaymentsReceived = paymentsReceived,
                ReservationsCount = reservationsCount,
                ServiceRequestsCount = serviceRequestsCount
            };

            _logger.LogInformation("Admin dashboard accessed");
            return View(model);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // Documents management
        public IActionResult Documents()
        {
            _logger.LogInformation("Admin documents section accessed");
            return RedirectToAction("AdminIndex", "Documents");
        }

        // Reports section
        public IActionResult Reports()
        {
            _logger.LogInformation("Admin reports section accessed");
            return View();
        }

        // API: Get all events as JSON for calendar
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _dbContext.Events
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    description = e.Description,
                    start = e.StartDate.ToString("yyyy-MM-dd'T'HH:mm:ss"),  // Changed from EventDate to StartDate
                    end = e.EndDate.ToString("yyyy-MM-dd'T'HH:mm:ss"),      // Use actual EndDate instead of adding hours
                    location = e.Location,
                    allDay = false
                })
                .ToListAsync();

            return Json(events);
        }

        // GET: Admin/CreateEvent
        public IActionResult CreateEvent()
        {
            return PartialView("_EventFormPartial", new Event());
        }

        // POST: Admin/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(Event eventItem)
        {
            if (ModelState.IsValid)
            {
                // Set the current admin as the creator
                var username = User.Identity?.Name;
                var admin = await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);

                if (admin != null)
                {
                    eventItem.CreatedById = admin.UserId;
                }

                eventItem.CreatedAt = DateTime.Now;
                eventItem.UpdatedAt = DateTime.Now;

                _dbContext.Add(eventItem);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Event created: {Title} by user {Username}", eventItem.Title, username);

                return Json(new { success = true, message = "Event created successfully" });
            }

            return Json(new { success = false, message = "Failed to create event", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // GET: Admin/GetEventDetails
        public async Task<IActionResult> GetEventDetails(int id)
        {
            var eventItem = await _dbContext.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            return Json(eventItem);
        }

        // GET: Admin/EditEvent
        public async Task<IActionResult> EditEvent(int id)
        {
            var eventItem = await _dbContext.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            return PartialView("_EventFormPartial", eventItem);
        }

        // POST: Admin/UpdateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEvent(Event eventItem)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get existing event to preserve creation data
                    var existingEvent = await _dbContext.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventItem.Id);
                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    // Update only the fields that should be updated
                    eventItem.CreatedById = existingEvent.CreatedById;
                    eventItem.CreatedAt = existingEvent.CreatedAt;
                    eventItem.UpdatedAt = DateTime.Now;

                    _dbContext.Update(eventItem);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Event updated: {Title} by user {Username}", eventItem.Title, User.Identity?.Name);

                    return Json(new { success = true, message = "Event updated successfully" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return Json(new { success = false, message = "Failed to update event", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        // POST: Admin/DeleteEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _dbContext.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            _dbContext.Events.Remove(eventItem);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Event deleted: {Title} by user {Username}", eventItem.Title, User.Identity?.Name);

            return Json(new { success = true, message = "Event deleted successfully" });
        }

        private bool EventExists(int id)
        {
            return _dbContext.Events.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeedbackDetails(int id)
        {
            try
            {
                var feedback = await _dbContext.Feedbacks
                    .Include(f => f.User)
                    .FirstOrDefaultAsync(f => f.FeedbackId == id);

                if (feedback == null)
                {
                    return NotFound(new { success = false, message = "Feedback not found" });
                }

                // Get responses for this feedback
                var responses = await _dbContext.FeedbackResponses
                    .Where(r => r.FeedbackId == id)
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => new
                    {
                        ResponseId = r.ResponseId,
                        ResponseText = r.ResponseText,
                        RespondedBy = r.RespondedBy,
                        RespondedAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    })
                    .ToListAsync();

                var result = new
                {
                    success = true,
                    feedback = new 
                    {
                        FeedbackId = feedback.FeedbackId,
                        UserId = feedback.UserId,
                        UserName = feedback.User != null ? feedback.User.FirstName + " " + feedback.User.LastName : "Unknown",
                        HouseNumber = feedback.User != null && feedback.User.Homeowner != null ? feedback.User.Homeowner.HouseNumber : string.Empty,
                        FeedbackType = feedback.FeedbackType,
                        Description = feedback.Description,
                        Status = feedback.Status,
                        CreatedAt = feedback.CreatedAt.ToString("yyyy-MM-dd"),
                        Responses = responses
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving feedback details for ID {id}");
                return StatusCode(500, new { success = false, message = $"Failed to retrieve feedback details: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFeedbackStatus([FromBody] FeedbackStatusUpdateModel model)
        {
            try
            {
                var feedback = await _dbContext.Feedbacks.FindAsync(model.Id);
                if (feedback == null)
                {
                    return NotFound(new { success = false, message = "Feedback not found" });
                }                // Validate status against database schema values (Submitted, In Review, In Progress, Resolved, Closed)
                if (model.Status != "Submitted" && model.Status != "In Review" && model.Status != "In Progress" && model.Status != "Resolved" && model.Status != "Closed")
                {
                    return BadRequest(new { success = false, message = "Invalid status value" });
                }

                feedback.Status = model.Status;
                feedback.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return Json(new { success = true, message = $"Status updated to {model.Status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating feedback status for ID {model.Id}");
                return StatusCode(500, new { success = false, message = "Failed to update feedback status" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFeedback([FromBody] FeedbackDeleteModel model)
        {
            try
            {
                var feedback = await _dbContext.Feedbacks.FindAsync(model.Id);
                if (feedback == null)
                {
                    return NotFound(new { success = false, message = "Feedback not found" });
                }

                // Delete related responses first
                var responses = await _dbContext.FeedbackResponses.Where(r => r.FeedbackId == model.Id).ToListAsync();
                _dbContext.FeedbackResponses.RemoveRange(responses);

                // Then delete the feedback
                _dbContext.Feedbacks.Remove(feedback);
                await _dbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Feedback deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feedback with ID {model.Id}");
                return StatusCode(500, new { success = false, message = "Failed to delete feedback" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFeedbackResponse([FromBody] FeedbackResponseModel model)
        {
            try
            {
                // Check if feedback exists
                var feedback = await _dbContext.Feedbacks.FindAsync(model.FeedbackId);
                if (feedback == null)
                {
                    return NotFound(new { success = false, message = "Feedback not found" });
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(model.ResponseText))
                {
                    return BadRequest(new { success = false, message = "Response text cannot be empty" });
                }

                // Get current admin name
                string adminName = User.Identity.Name ?? "Administrator";

                // Create new response
                var response = new FeedbackResponse
                {
                    FeedbackId = model.FeedbackId,
                    ResponseText = model.ResponseText,
                    RespondedBy = adminName,
                    CreatedAt = DateTime.Now
                };

                _dbContext.FeedbackResponses.Add(response);                // Update the feedback status to "In Progress" if it's currently "Submitted"
                if (feedback.Status == "Submitted")
                {
                    feedback.Status = "In Progress";
                    feedback.UpdatedAt = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();

                // Return formatted response for display
                return Json(new
                {
                    success = true,
                    message = "Response added successfully",
                    response = new
                    {
                        ResponseId = response.ResponseId,
                        ResponseText = response.ResponseText,
                        RespondedBy = response.RespondedBy,
                        RespondedAt = response.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding feedback response for ID {model.FeedbackId}");
                return StatusCode(500, new { success = false, message = "Failed to add response" });
            }
        }
    }
}