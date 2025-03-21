using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using daebak_subdivision_website.Models;
using daebak_subdivision_website.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace daebak_subdivision_website.Controllers
{
    public class FacilitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FacilitiesController> _logger;

        public FacilitiesController(ApplicationDbContext context, ILogger<FacilitiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Facilities/BookFacility
        public IActionResult BookFacility()
        {
            try
            {
                // Even if there's an error loading facilities, we still want to show the page
                // We'll attempt to load facilities, but won't let it prevent the page from displaying
                var facilities = _context.Facilities.Where(f => f.Status == "Available").ToList();
                
                ViewBag.UserName = "Homeowner"; // Set the same ViewBag as HomeController for consistency
                
                // Always return the Facilities view
                return View("~/Views/Home/Facilities.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading facilities");
                
                // Still show the Facilities page even if we couldn't load data
                ViewBag.UserName = "Homeowner";
                ViewBag.ErrorMessage = "There was an error loading facility data. Some features may be limited.";
                
                return View("~/Views/Home/Facilities.cshtml");
            }
        }

        // POST: /Facilities/BookFacility
        [HttpPost]
        public async Task<IActionResult> BookFacility(FacilityReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableFacilities = await _context.Facilities.ToListAsync();
                return View("~/Views/Home/Facilities.cshtml");
            }

            try
            {
                // Check if user is authenticated
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("BookFacility", "Facilities") });
                }
                
                // Get current user ID (with safer parsing)
                int userId;
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim, out userId))
                {
                    // For development/demo purposes, use a default userID
                    userId = 1; // Use a default value for testing
                    _logger.LogWarning("User ID not found in claims. Using default value.");
                }
                
                // Check for existing reservations on the same date/time
                var existingReservation = await _context.FacilityReservations
                    .AnyAsync(r => r.FacilityId == model.FacilityId && 
                              r.ReservationDate == model.ReservationDate &&
                              r.Status != "Cancelled");

                if (existingReservation)
                {
                    ModelState.AddModelError("", "This facility is already reserved for the selected time.");
                    model.AvailableFacilities = await _context.Facilities.ToListAsync();
                    return View("~/Views/Home/Facilities.cshtml");
                }

                // Create new reservation
                var reservation = new FacilityReservation
                {
                    FacilityId = model.FacilityId,
                    UserId = userId,
                    ReservationDate = model.ReservationDate,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.FacilityReservations.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your reservation has been created successfully!";
                return RedirectToAction("BookFacility");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return RedirectToAction("Error", "Home", new { message = "Error creating reservation. Please try again later." });
            }
        }

        // GET: /Facilities/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            try
            {
                // Check if user is authenticated
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("MyReservations", "Facilities") });
                }
                
                // Get current user ID (with safer parsing)
                int userId;
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim, out userId))
                {
                    // For development/demo purposes, use a default userID
                    userId = 1; // Use a default value for testing
                    _logger.LogWarning("User ID not found in claims. Using default value.");
                }
                
                var reservations = await _context.FacilityReservations
                    .Include(r => r.Facility)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.ReservationDate)
                    .Select(r => new FacilityReservationViewModel
                    {
                        ReservationId = r.ReservationId,
                        FacilityId = r.FacilityId,
                        UserId = r.UserId,
                        ReservationDate = r.ReservationDate,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        FacilityName = r.Facility.Name
                    })
                    .ToListAsync();

                // Use the pre-existing Facilities view with the reservations tab active
                return View("~/Views/Home/Facilities.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reservations");
                return RedirectToAction("Error", "Home", new { message = "Error loading reservations. Please try again later." });
            }
        }

        // POST: /Facilities/CancelReservation
        [HttpPost]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                // Check if user is authenticated
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                // Get current user ID (with safer parsing)
                int userId;
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim, out userId))
                {
                    // For development/demo purposes, use a default userID
                    userId = 1; // Use a default value for testing
                    _logger.LogWarning("User ID not found in claims. Using default value.");
                }
                
                var reservation = await _context.FacilityReservations
                    .FirstOrDefaultAsync(r => r.ReservationId == id && r.UserId == userId);
                
                if (reservation == null)
                {
                    return NotFound();
                }

                // Check if the reservation date is within 48 hours
                bool isWithin48Hours = reservation.ReservationDate <= DateTime.Now.AddHours(48);
                
                // Update reservation status
                reservation.Status = "Cancelled";
                reservation.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
                
                // If cancelled within 48 hours, potentially add a penalty fee
                if (isWithin48Hours)
                {
                    TempData["Message"] = "Your reservation has been cancelled. Please note that cancellations within 48 hours may incur a penalty fee.";
                }
                else
                {
                    TempData["Message"] = "Your reservation has been successfully cancelled.";
                }

                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation");
                return RedirectToAction("Error", "Home", new { message = "Error cancelling reservation. Please try again later." });
            }
        }

        // GET: /Facilities/GetAvailableTimes
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int facilityId, DateTime date)
        {
            try
            {
                // Get existing reservations for the selected facility and date
                var reservations = await _context.FacilityReservations
                    .Where(r => r.FacilityId == facilityId && 
                           r.ReservationDate.Date == date.Date && 
                           r.Status != "Cancelled")
                    .Select(r => new { r.ReservationDate })
                    .ToListAsync();

                return Json(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load available times");
                return Json(new { error = "Failed to load available times" });
            }
        }
    }
}
