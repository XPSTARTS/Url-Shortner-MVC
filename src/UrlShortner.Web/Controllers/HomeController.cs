// src/UrlShortner.Web/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;

namespace UrlShortner.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // If user is logged in, show shorten form with their context
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