// src/UrlShortner.Web/Controllers/RedirectController.cs
using Microsoft.AspNetCore.Mvc;
using UrlShortner.Application.Services;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Web.Controllers;

public class RedirectController : Controller
{
    private readonly UrlShorteningService _urlShorteningService;
    private readonly IClickLogRepository _clickLogRepository;

    public RedirectController(
        UrlShorteningService urlShorteningService,
        IClickLogRepository clickLogRepository)
    {
        _urlShorteningService = urlShorteningService;
        _clickLogRepository = clickLogRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string shortCode)
    {
        if (string.IsNullOrEmpty(shortCode))
            return RedirectToAction("Index", "Home");

        // Skip reserved paths
        if (IsReservedPath(shortCode))
            return NotFound();

        var originalUrl = await _urlShorteningService.GetOriginalUrlAsync(shortCode);

        if (originalUrl == null)
            return NotFound();

        // Record click
        _ = Task.Run(async () =>
        {
            try
            {
                var clickLog = new ClickLog
                {
                    ShortUrlId = 0,
                    ClickedAt = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Referrer = Request.Headers["Referer"].ToString()
                };
                await _clickLogRepository.CreateAsync(clickLog);
            }
            catch { }
        });

        return RedirectPermanent(originalUrl);
    }

    private bool IsReservedPath(string path)
    {
        var reservedPaths = new[]
        {
            "dashboard", "auth", "url", "urls", "home", "health",
            "api", "css", "js", "lib", "images", "favicon.ico",
            "register", "login", "verifyotp", "profile", "result",
            "create", "index", "error"
        };

        return reservedPaths.Contains(path.ToLower()) ||
               path.Contains('.') ||
               path.Length > 50;
    }
}