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
            return View(announcements);
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
    }
}