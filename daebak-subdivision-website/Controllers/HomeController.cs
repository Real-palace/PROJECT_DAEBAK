using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using daebak_subdivision_website.Models;

namespace daebak_subdivision_website.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // This will show the login page
        return View(new LoginViewModel());
    }

    // This action will show the home page after successful login
    public IActionResult Home()
    {
        // Adding some sample data for the home page
        ViewBag.UserName = "Homeowner";
        ViewBag.DueBills = 2;
        ViewBag.EventCount = 3;
        ViewBag.RequestCount = 1;
        
        // Sample calendar events for the home page
        ViewBag.Events = new[]
        {
            new { title = "Homeowners Meeting", start = "2023-05-15", color = "#FF9AA2" },
            new { title = "Facility Maintenance", start = "2023-05-20", color = "#B5EAD7" },
            new { title = "Community Clean-Up", start = "2023-05-25", color = "#C7CEEA" }
        };
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
