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
        // Initialize a new LoginViewModel to prevent null reference exceptions
        return View(new LoginViewModel());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        // This would be the page users see after successful login
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
