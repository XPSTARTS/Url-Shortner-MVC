// src/UrlShortner.Web/Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UrlShortner.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger , IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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

    [HttpGet("/debug")]
    public IActionResult Debug()
    {
        var redis = Environment.GetEnvironmentVariable("REDIS_CONNECTION") ?? "NOT SET";
        var db = Environment.GetEnvironmentVariable("DB_CONNECTION") ?? "NOT SET";
        var jwt = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? "NOT SET";

        var configRedis = _configuration.GetConnectionString("Redis") ?? "NOT SET";
        var configDb = _configuration.GetConnectionString("DefaultConnection") ?? "NOT SET";

        return Ok(new
        {
            env_redis = redis,
            env_db = db,
            env_jwt = jwt,
            config_redis = configRedis,
            config_db = configDb
        });
    }
}