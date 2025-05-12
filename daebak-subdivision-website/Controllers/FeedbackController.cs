using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using daebak_subdivision_website.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace daebak_subdivision_website.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FeedbackController> _logger;

        public FeedbackController(ApplicationDbContext context, ILogger<FeedbackController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "HOMEOWNER")]
        public async Task<IActionResult> Create(string FeedbackType, string Description)
        {
            try
            {
                // Get current user ID
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not found. Please log in again.";
                    return RedirectToAction("Dashboard", "Homeowner");
                }
                
                // Basic validation
                if (string.IsNullOrEmpty(FeedbackType) || string.IsNullOrEmpty(Description))
                {
                    TempData["ErrorMessage"] = "Feedback type and description are required.";
                    return RedirectToAction("Dashboard", "Homeowner");
                }
                
                // Create new feedback
                var feedback = new Feedback
                {
                    UserId = int.Parse(userId),
                    FeedbackType = FeedbackType,
                    Description = Description,
                    Status = "Submitted",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                    // Do not set HouseNumber if it's not in your database schema
                };

                // Add to database
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                
                // Success message
                TempData["SuccessMessage"] = "Your feedback has been submitted successfully.";
                return RedirectToAction("Dashboard", "Homeowner");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting feedback");
                TempData["ErrorMessage"] = "There was an error submitting your feedback. Please try again.";
                return RedirectToAction("Dashboard", "Homeowner");
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index()
        {
            var feedbackList = await _context.Feedbacks
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
                
            return View(feedbackList);
        }
        
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Details(int id)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.FeedbackId == id);
                
            if (feedback == null)
            {
                return NotFound();
            }
            
            return View(feedback);
        }
        
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            
            feedback.Status = status;
            feedback.UpdatedAt = DateTime.Now; // Use DateTime.Now directly
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Feedback status updated successfully.";
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View("~/Views/Management/Feedback.cshtml");
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
                _logger.LogError(ex, $"Error deleting feedback with ID {id}");
                ModelState.AddModelError("", "Error deleting feedback: " + ex.Message);
                return RedirectToAction("Index");
            }
        }

        // Admin feedback management methods
        
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [Route("Admin/Feedback")]
        public async Task<IActionResult> AdminFeedback()
        {
            try
            {
                var model = new AdminPageModel();
                
                // Get feedback statistics
                model.FeedbackStats = new                {
                    OpenCount = await _context.Feedbacks.CountAsync(f => f.Status == "Submitted"),
                    InProgressCount = await _context.Feedbacks.CountAsync(f => f.Status == "In Progress" || f.Status == "In Review"),
                    ResolvedCount = await _context.Feedbacks.CountAsync(f => f.Status == "Resolved" || f.Status == "Closed"),
                    TotalCount = await _context.Feedbacks.CountAsync()
                };
                
                // Get all feedback items
                var feedbacks = await _context.Feedbacks
                    .Include(f => f.User)
                    .ThenInclude(u => u.Homeowner)
                    .OrderByDescending(f => f.CreatedAt)
                    .Select(f => new FeedbackViewModel
                    {
                        FeedbackId = f.FeedbackId,
                        UserId = f.UserId,
                        UserName = f.User != null ? f.User.FirstName + " " + f.User.LastName : "Unknown",
                        HouseNumber = f.User != null && f.User.Homeowner != null ? f.User.Homeowner.HouseNumber : string.Empty,
                        FeedbackType = f.FeedbackType,
                        Description = f.Description,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();
                
                model.Feedbacks = feedbacks;
                
                return View("~/Views/Admin/Feedback.cshtml", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin feedback page");
                return View("~/Views/Shared/Error.cshtml", new ErrorViewModel { RequestId = ex.Message });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [Route("Admin/GetAllFeedback")]
        public async Task<IActionResult> GetAllFeedback()
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
                        HouseNumber = f.User != null && f.User.Homeowner != null ? f.User.Homeowner.HouseNumber : string.Empty,  // Get HouseNumber from User.Homeowner instead
                        FeedbackType = f.FeedbackType,
                        Description = f.Description,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd")
                    })
                    .OrderByDescending(f => f.CreatedAt)
                    .ToListAsync();

                return Json(feedbackList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feedback data");
                return StatusCode(500, new { success = false, message = "Failed to retrieve feedback data" });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [Route("Admin/Feedback/GetDetails/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks
                    .Include(f => f.User)
                    .ThenInclude(u => u.Homeowner)  // Make sure to include Homeowner
                    .FirstOrDefaultAsync(f => f.FeedbackId == id);

                if (feedback == null)
                {
                    _logger.LogWarning($"Feedback with ID {id} not found");
                    return Json(new { success = false, message = $"Feedback #{id} not found" });
                }

                // Format the house number if available
                string houseNumber = "Not specified";
                if (feedback.User?.Homeowner != null)
                {
                    houseNumber = feedback.User.Homeowner.HouseNumber;
                }

                return Json(new
                {
                    success = true,
                    feedback = new
                    {
                        feedbackId = feedback.FeedbackId,
                        userId = feedback.UserId,
                        userName = $"{feedback.User?.FirstName} {feedback.User?.LastName}",
                        houseNumber = houseNumber,
                        feedbackType = feedback.FeedbackType,
                        description = feedback.Description,
                        status = feedback.Status,
                        createdAt = feedback.CreatedAt,
                        updatedAt = feedback.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving feedback details for ID {id}");
                return StatusCode(500, new { success = false, message = "An error occurred while retrieving feedback details" });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Feedback/Admin/UpdateFeedbackStatus")]
        public async Task<IActionResult> UpdateFeedbackStatus(int id, string status)
        {
            try
            {
                _logger.LogInformation($"Updating feedback status - ID: {id}, Status: {status}");
                
                var feedback = await _context.Feedbacks.FindAsync(id);
                
                if (feedback == null)
                {
                    _logger.LogWarning($"Failed to update status: Feedback with ID {id} not found");
                    return Json(new { success = false, message = "Feedback not found" });
                }
                
                // Validate status against check constraint in database
                if (status != "Submitted" && status != "In Review" && status != "In Progress" && 
                    status != "Resolved" && status != "Closed")
                {
                    _logger.LogWarning($"Invalid status value: {status}");
                    return Json(new { success = false, message = "Invalid status value" });
                }

                feedback.Status = status;
                feedback.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Successfully updated feedback {id} status to {status}");
                return Json(new { success = true, message = $"Status updated to {status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating feedback status for ID {id}");
                return Json(new { success = false, message = "An error occurred while updating status" });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Feedback/Admin/DeleteFeedback")]
        public async Task<IActionResult> AdminDeleteFeedback(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null)
                {
                    return NotFound(new { success = false, message = "Feedback not found" });
                }

                // Simply delete the feedback (no responses to worry about)
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Feedback deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feedback with ID {id}");
                return StatusCode(500, new { success = false, message = "Failed to delete feedback" });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [Route("Admin/ExportFeedback")]
        public async Task<IActionResult> ExportFeedback()
        {
            try
            {
                var feedbackList = await _context.Feedbacks
                    .Include(f => f.User)
                    .ThenInclude(u => u.Homeowner)
                    .Select(f => new
                    {
                        FeedbackId = f.FeedbackId,
                        UserName = f.User != null ? f.User.FirstName + " " + f.User.LastName : "Unknown",
                        HouseNumber = f.User != null && f.User.Homeowner != null ? f.User.Homeowner.HouseNumber : string.Empty, // Get from User.Homeowner
                        FeedbackType = f.FeedbackType,
                        Description = f.Description,
                        Status = f.Status,
                        CreatedAt = f.CreatedAt.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                // Create CSV content
                var csv = "Feedback ID,User Name,House Number,Type,Description,Status,Created At\n";
                foreach (var item in feedbackList)
                {
                    // Properly escape fields that might contain commas
                    string description = $"\"{item.Description.Replace("\"", "\"\"")}\"";
                    csv += $"{item.FeedbackId},{item.UserName},{item.HouseNumber},{item.FeedbackType},{description},{item.Status},{item.CreatedAt}\n";
                }

                // Return as file download
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(csv);
                return File(bytes, "text/csv", $"feedback_export_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting feedback data");
                return StatusCode(500, new { success = false, message = "Failed to export feedback data" });
            }
        }

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [Route("Admin/GetFeedbackStats")]
        public async Task<IActionResult> GetFeedbackStats()
        {
            try
            {                var stats = new
                {
                    OpenCount = await _context.Feedbacks.CountAsync(f => f.Status == "Submitted"),
                    InProgressCount = await _context.Feedbacks.CountAsync(f => f.Status == "In Progress" || f.Status == "In Review"),
                    ResolvedCount = await _context.Feedbacks.CountAsync(f => f.Status == "Resolved" || f.Status == "Closed"),
                    TotalCount = await _context.Feedbacks.CountAsync()
                };

                return Json(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feedback statistics");
                return StatusCode(500, new { success = false, message = "Failed to retrieve feedback statistics" });
            }
        }
    }
}