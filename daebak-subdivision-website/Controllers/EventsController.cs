using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using daebak_subdivision_website.Models;
using System.Text.Json;
using System.Security.Claims;

namespace daebak_subdivision_website.Controllers
{
    [Authorize]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public EventsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: /Events/Index
        public async Task<IActionResult> Index()
        {
            var events = await _dbContext.Events
                .Include(e => e.CreatedBy)
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }

        // GET: /Events/GetEvents
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            try
            {
                var events = await _dbContext.Events.ToListAsync();

                var calendarEvents = events.Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = e.Description,
                    location = e.Location,
                    backgroundColor = GetEventColor(e.Title),
                    borderColor = GetEventColor(e.Title)
                });

                return Json(calendarEvents);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: /Events/CreateEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent([FromBody] EventViewModel eventViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int currentUserId = 0;
                if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int parsedUserId))
                {
                    currentUserId = parsedUserId;
                }

                // Combine date and time for start and end
                DateTime startDateTime, endDateTime;

                // Parse the date and time inputs
                if (!DateTime.TryParse($"{eventViewModel.StartDate} {eventViewModel.StartTime}", out startDateTime))
                {
                    return BadRequest(new { success = false, message = "Invalid start date or time format." });
                }

                if (!DateTime.TryParse($"{eventViewModel.EndDate} {eventViewModel.EndTime}", out endDateTime))
                {
                    return BadRequest(new { success = false, message = "Invalid end date or time format." });
                }

                // Validate that end date is after start date
                if (endDateTime <= startDateTime)
                {
                    return BadRequest(new { success = false, message = "End date must be after start date." });
                }

                // Create the new event - Description is now optional
                var newEvent = new Event
                {
                    Title = eventViewModel.Title,
                    Description = eventViewModel.Description ?? "", // Handle null Description
                    StartDate = startDateTime,
                    EndDate = endDateTime,
                    Location = eventViewModel.Location,
                    CreatedById = currentUserId > 0 ? (int?)currentUserId : null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _dbContext.Events.Add(newEvent);
                await _dbContext.SaveChangesAsync();

                // Return success response with the created event
                return Json(new
                {
                    success = true,
                    message = "Event created successfully!",
                    id = newEvent.Id,
                    title = newEvent.Title,
                    start = newEvent.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = newEvent.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    description = newEvent.Description,
                    location = newEvent.Location
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _dbContext.Events
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // GET: /Events/Edit/5
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _dbContext.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: /Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Edit(int id, Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    eventItem.UpdatedAt = DateTime.Now;
                    _dbContext.Update(eventItem);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
            return View(eventItem);
        }

        // GET: /Events/Delete/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _dbContext.Events
                .Include(e => e.CreatedBy)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: /Events/DeleteConfirmed/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _dbContext.Events.FindAsync(id);
            if (eventItem != null)
            {
                _dbContext.Events.Remove(eventItem);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _dbContext.Events.Any(e => e.Id == id);
        }

        // Helper to assign colors based on event title
        private string GetEventColor(string eventTitle)
        {
            // Default color
            string color = "#356480";

            if (eventTitle == null)
                return color;

            eventTitle = eventTitle.ToLower();

            // Assign colors based on keywords in title
            if (eventTitle.Contains("meeting"))
                color = "#FF9AA2"; // Red
            else if (eventTitle.Contains("maintenance"))
                color = "#B5EAD7"; // Green
            else if (eventTitle.Contains("clean"))
                color = "#C7CEEA"; // Blue
            else if (eventTitle.Contains("due"))
                color = "#FF6B6B"; // Red
            else if (eventTitle.Contains("party") || eventTitle.Contains("celebration"))
                color = "#FFB347"; // Orange
            else if (eventTitle.Contains("community") || eventTitle.Contains("gathering"))
                color = "#E2F0CB"; // Lime
            else if (eventTitle.Contains("sport"))
                color = "#FFDAC1"; // Peach
            else
                color = "#94B0DF"; // Default color when no keywords match

            return color;
        }
    }

    public class EventViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public string Location { get; set; }
        public string Color { get; set; }
    }
}