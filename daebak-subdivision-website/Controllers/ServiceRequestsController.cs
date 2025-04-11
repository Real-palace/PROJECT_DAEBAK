using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System.Collections.Generic;
using System.Security.Claims;

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
        var serviceRequests = _context.ServiceRequests
            .Where(sr => sr.UserId == int.Parse(userId)) // Filter by user ID
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
}
