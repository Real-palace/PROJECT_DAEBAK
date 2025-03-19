using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using daebak_subdivision_website.Models;
using System.Collections.Generic;

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
            .Join(_context.Users,
                sr_u => sr_u.sr.AssignedTo,
                u => u.UserId,
                (sr_u, assigned) => new ServiceRequestView
                {
                    Id = sr_u.sr.Id,
                    UserId = sr_u.sr.UserId,
                    HouseNumber = sr_u.sr.HouseNumber,
                    RequestType = sr_u.sr.RequestType,
                    Description = sr_u.sr.Description,
                    Status = sr_u.sr.Status,
                    CreatedAt = sr_u.sr.CreatedAt,
                    UpdatedAt = sr_u.sr.UpdatedAt,
                    AssignedTo = sr_u.sr.AssignedTo,
                    RequestedBy = sr_u.RequestedBy,
                    AssignedToName = assigned.FirstName + " " + assigned.LastName
                })
            .ToList();

        return View("~/Views/Management/ServiceRequests.cshtml", serviceRequests);
    }
}
