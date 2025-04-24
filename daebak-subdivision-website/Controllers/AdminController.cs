using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daebak_subdivision_website.Models;
using Microsoft.Extensions.Logging;

namespace daebak_subdivision_website.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext dbContext, ILogger<AdminController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public IActionResult Dashboard()
        {
            _logger.LogInformation("Admin dashboard accessed");
            return View(new AdminPageModel());
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}