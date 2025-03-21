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

    // Billing action method to display the billing page
    public IActionResult Billing()
    {
        // Set demo values for the view
        ViewBag.UserName = "John Doe";
        ViewBag.HouseNumber = "Lot 45";
        ViewBag.CurrentBalance = "5,250.00";
        ViewBag.DueDate = "March 15, 2025";
        ViewBag.LastPayment = "2,500.00";
        ViewBag.LastPaymentDate = "February 15, 2025";

        return View();
    }

    // PayBill action to redirect to the payment section of the billing page
    public IActionResult PayBill()
    {
        // Redirect to the Billing page with a fragment identifier to open the payment tab
        return Redirect(Url.Action("Billing") + "#make-payment");
    }

    // Add this new action method for the Services page
    public IActionResult Services()
    {
        // Set default user name for the view header
        ViewBag.UserName = "Homeowner";
        return View();
    }

    // Redirect from old Facilities page to the new FacilitiesController
    public IActionResult Facilities()
    {
        return RedirectToAction("BookFacility", "Facilities");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}