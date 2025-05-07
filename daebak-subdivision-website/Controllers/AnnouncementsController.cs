using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace daebak_subdivision_website.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _context.ANNOUNCEMENTS
                .Include(a => a.CreatedByUser)
                .OrderByDescending(a => a.CREATED_AT)
                .ToListAsync();
            return View("~/Views/Admin/Announcements.cshtml", announcements);
        }

        // Special route for admin dashboard
        [Route("Admin/Announcements")]
        public async Task<IActionResult> AdminIndex()
        {
            var announcements = await _context.ANNOUNCEMENTS
                .Include(a => a.CreatedByUser)
                .OrderByDescending(a => a.CREATED_AT)
                .ToListAsync();
            return View("~/Views/Admin/Announcements.cshtml", announcements);
        }

        // GET: Announcement details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.ANNOUNCEMENTS
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(m => m.ANNOUNCEMENT_ID == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                // Set creation/update timestamps
                announcement.CREATED_AT = DateTime.Now;
                announcement.UPDATED_AT = DateTime.Now;

                // Set creator ID from current user
                var userId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    announcement.CREATED_BY = int.Parse(userId);
                }

                _context.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // AJAX endpoint for creating announcements
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateAjax([FromBody] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Set creation/update timestamps
                    announcement.CREATED_AT = DateTime.Now;
                    announcement.UPDATED_AT = DateTime.Now;

                    // Set creator ID from current user
                    var userId = User.FindFirst("UserId")?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        announcement.CREATED_BY = int.Parse(userId);
                    }

                    _context.Add(announcement);
                    await _context.SaveChangesAsync();

                    var createdBy = "Administrator";
                    if (announcement.CreatedByUser != null)
                    {
                        createdBy = $"{announcement.CreatedByUser.FirstName} {announcement.CreatedByUser.LastName}";
                    }

                    return Json(new
                    {
                        success = true,
                        id = announcement.ANNOUNCEMENT_ID,
                        title = announcement.TITLE,
                        content = announcement.CONTENT,
                        category = announcement.Category,
                        categoryColor = announcement.CategoryColor,
                        createdAt = announcement.CREATED_AT.ToString("MMM dd, yyyy"),
                        createdBy = createdBy
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // If ModelState is invalid, return the errors
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, message = "Validation failed", errors });
        }

        // GET: Edit announcement
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.ANNOUNCEMENTS.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Edit announcement
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement announcement)
        {
            if (id != announcement.ANNOUNCEMENT_ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the timestamp
                    announcement.UPDATED_AT = DateTime.Now;

                    // Preserve the original creation date and creator
                    var originalAnnouncement = await _context.ANNOUNCEMENTS.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.ANNOUNCEMENT_ID == id);

                    if (originalAnnouncement != null)
                    {
                        announcement.CREATED_AT = originalAnnouncement.CREATED_AT;
                        announcement.CREATED_BY = originalAnnouncement.CREATED_BY;
                    }

                    _context.Update(announcement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementExists(announcement.ANNOUNCEMENT_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // AJAX endpoint for editing announcements
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> EditAjax([FromBody] Announcement announcement)
        {
            if (!AnnouncementExists(announcement.ANNOUNCEMENT_ID))
            {
                return Json(new { success = false, message = "Announcement not found" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the timestamp
                    announcement.UPDATED_AT = DateTime.Now;

                    // Preserve the original creation date and creator
                    var originalAnnouncement = await _context.ANNOUNCEMENTS.AsNoTracking()
                        .FirstOrDefaultAsync(a => a.ANNOUNCEMENT_ID == announcement.ANNOUNCEMENT_ID);

                    if (originalAnnouncement != null)
                    {
                        announcement.CREATED_AT = originalAnnouncement.CREATED_AT;
                        announcement.CREATED_BY = originalAnnouncement.CREATED_BY;
                    }

                    _context.Update(announcement);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        id = announcement.ANNOUNCEMENT_ID,
                        title = announcement.TITLE,
                        content = announcement.CONTENT,
                        category = announcement.Category,
                        categoryColor = announcement.CategoryColor,
                        updatedAt = DateTime.Now.ToString("MMM dd, yyyy HH:mm:ss")
                    });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }

            // If ModelState is invalid, return the errors
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, message = "Validation failed", errors });
        }

        // GET: Delete announcement
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.ANNOUNCEMENTS
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(m => m.ANNOUNCEMENT_ID == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Confirm delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.ANNOUNCEMENTS.FindAsync(id);
            if (announcement != null)
            {
                _context.ANNOUNCEMENTS.Remove(announcement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX endpoint for deleting announcements
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteAjax([FromBody] int id)
        {
            try
            {
                var announcement = await _context.ANNOUNCEMENTS.FindAsync(id);
                if (announcement == null)
                {
                    return Json(new { success = false, message = "Announcement not found" });
                }

                _context.ANNOUNCEMENTS.Remove(announcement);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to check if an announcement exists
        private bool AnnouncementExists(int id)
        {
            return _context.ANNOUNCEMENTS.Any(e => e.ANNOUNCEMENT_ID == id);
        }
    }
}