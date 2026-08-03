// src/UrlShortner.Web/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UrlShortner.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Home page visited");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Terms()
    {
        return View();
    }

    [HttpGet("/Home/Error")]
    public IActionResult Error(int statusCode)
    {
        if (statusCode == 404)
        {
            return View("NotFound");
        }

        return View("Error");
    }

}