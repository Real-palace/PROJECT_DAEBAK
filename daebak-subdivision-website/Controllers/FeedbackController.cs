using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace daebak_subdivision_website.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var feedbackList = await _context.Feedbacks
                    .Include(f => f.User)
                    .Select(f => new FeedbackViewModel
                    {
                        FeedbackId = f.FeedbackId,
                        UserId = f.UserId,
                        UserName = f.User != null ? f.User.FirstName + " " + f.User.LastName : "Unknown",
                        HouseNumber = f.HouseNumber,
                        FeedbackType = f.FeedbackType,
                        Description = f.Description,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                return View("~/Views/Management/Feedback.cshtml", feedbackList);
            }
            catch (Exception ex)
            {
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        public IActionResult Create()
        {
            return View("~/Views/Management/Feedback.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Feedback model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Management/Feedback.cshtml", model);
            }

            try
            {
                model.CreatedAt = DateTime.Now;
                model.UpdatedAt = DateTime.Now;
                model.Status = "Open";

                _context.Feedbacks.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving feedback: " + ex.Message);
                return View("~/Views/Management/Feedback.cshtml", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback != null)
                {
                    _context.Feedbacks.Remove(feedback);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error deleting feedback: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}
