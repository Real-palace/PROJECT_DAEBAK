using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using daebak_subdivision_website.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace daebak_subdivision_website.Controllers
{
    public class FacilityReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacilityReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all reservations with facility and user details
        public async Task<IActionResult> Index()
        {
            try
            {
                var reservations = await _context.FacilityReservations
                    .Include(r => r.Facility)
                    .Include(r => r.User)
                    .Select(r => new FacilityReservationViewModel
                    {
                        ReservationId = r.ReservationId, // FIXED: Changed r.Id to r.ReservationId
                        FacilityId = r.FacilityId,
                        UserId = r.UserId,
                        ReservationDate = r.ReservationDate,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        FacilityName = r.Facility.Name,
                        UserName = r.User.FirstName + " " + r.User.LastName
                    })
                    .ToListAsync();

                return View("~/Views/Management/FacilityReservations.cshtml", reservations);
            }
            catch (Exception ex)
            {
                // Log error (if you have a logging mechanism)
                ModelState.AddModelError("", "Error loading reservations: " + ex.Message);
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        // Load the Create Reservation Form with available facilities
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = new FacilityReservationViewModel
                {
                    AvailableFacilities = await _context.Facilities.ToListAsync()
                };
                return View("~/Views/Management/FacilityReservations.cshtml", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading facilities: " + ex.Message);
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        // Handle new reservation submission
        [HttpPost]
        public async Task<IActionResult> Create(FacilityReservationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableFacilities = await _context.Facilities.ToListAsync();
                return View("~/Views/Management/FacilityReservations.cshtml", model);
            }

            try
            {
                var reservation = new FacilityReservation
                {
                    FacilityId = model.FacilityId,
                    UserId = model.UserId,
                    ReservationDate = model.ReservationDate,
                    Status = model.Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.FacilityReservations.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving reservation: " + ex.Message);
                model.AvailableFacilities = await _context.Facilities.ToListAsync();
                return View("~/Views/Management/FacilityReservations.cshtml", model);
            }
        }

        // Handle reservation deletion
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var reservation = await _context.FacilityReservations.FindAsync(id);
                if (reservation != null)
                {
                    _context.FacilityReservations.Remove(reservation);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error deleting reservation: " + ex.Message);
                return RedirectToAction("Index");
            }
        }
    }
}
