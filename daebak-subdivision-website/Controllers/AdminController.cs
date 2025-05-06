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
    }
}