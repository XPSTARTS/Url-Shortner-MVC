// src/UrlShortner.Web/Controllers/ErrorController.cs
using Microsoft.AspNetCore.Mvc;

namespace UrlShortner.Web.Controllers;

public class ErrorController : Controller
{
    [Route("/Error/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        ViewBag.StatusCode = statusCode;
        ViewBag.Message = statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Page Not Found",
            500 => "Internal Server Error",
            _ => "An error occurred"
        };

        return View();
    }
}