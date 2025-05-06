using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

public class ServiceRequestsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

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
                HouseNumber = x.sr_u.sr.HouseNumber,
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

        // Debugging: Check the count of service requests
        if (serviceRequests.Count == 0)
        {
            // Log or debug output
            Console.WriteLine("No service requests found.");
        }

        return View("~/Views/Management/ServiceRequests.cshtml", serviceRequests);
    }

    public IActionResult TrackRequests()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID
        if (int.TryParse(userId, out int userIdInt))
        {
            var serviceRequests = _context.ServiceRequests
                .Where(sr => sr.UserId == userIdInt) // Fixed: Using parsed int value
                .Select(sr => new ServiceRequestView
                {
                    Id = sr.Id,
                    RequestType = sr.RequestType,
                    Description = sr.Description,
                    Status = sr.Status,
                    CreatedAt = sr.CreatedAt
                })
                .ToList();

            return View(serviceRequests); // Pass the data to the view
        }

        return View(new List<ServiceRequestView>()); // Return empty list if userId can't be parsed
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
                HouseNumber = x.sr_u.sr.HouseNumber,
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

    // Add these API endpoints for admin operations
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/ServiceRequests/UpdateStatus")]
    public IActionResult UpdateServiceRequestStatus(int id, string status, string adminResponse)
    {
        var serviceRequest = _context.ServiceRequests.Find(id);
        if (serviceRequest == null)
        {
            return NotFound();
        }

        serviceRequest.Status = status;
        serviceRequest.AdminResponse = adminResponse;
        serviceRequest.UpdatedAt = System.DateTime.Now;
        
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpDelete]
    [Authorize(Roles = "ADMIN")]
    [Route("Admin/ServiceRequests/Delete/{id}")]
    public IActionResult DeleteServiceRequest(int id)
    {
        var serviceRequest = _context.ServiceRequests.Find(id);
        if (serviceRequest == null)
        {
            return NotFound();
        }

        _context.ServiceRequests.Remove(serviceRequest);
        _context.SaveChanges();
        return Json(new { success = true });
    }
}
