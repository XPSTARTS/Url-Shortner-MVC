// src/UrlShortner.Web/Controllers/RedirectController.cs
using Microsoft.AspNetCore.Mvc;
using UrlShortner.Application.Services;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;
using UrlShortner.Infrastructure.Repositories;  

namespace UrlShortner.Web.Controllers;

public class RedirectController : Controller
{
    private readonly UrlShorteningService _urlShorteningService;
    private readonly IClickLogRepository _clickLogRepository;
    private readonly IShortUrlRepository _shortUrlRepository;  

    public RedirectController(
        UrlShorteningService urlShorteningService,
        IClickLogRepository clickLogRepository,
        IShortUrlRepository shortUrlRepository)  
    {
        _urlShorteningService = urlShorteningService;
        _clickLogRepository = clickLogRepository;
        _shortUrlRepository = shortUrlRepository;  
    }

    [HttpGet]
    public async Task<IActionResult> Index(string shortCode)
    {
        if (IsReservedPath(shortCode))
            return NotFound();

        var url = await _shortUrlRepository.GetByCodeAsync(shortCode);

        if (url == null)
            return NotFound();

        // Check if URL is expired
        if (url.ExpiresAt != null && url.ExpiresAt < DateTime.UtcNow)
            return NotFound();

        // 🔑 Increment click count (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await _shortUrlRepository.IncrementClickCountAsync(url.Id);
            }
            catch { }
        });

        _ = Task.Run(async () =>
        {
            try
            {
                var clickLog = new ClickLog
                {
                    ShortUrlId = url.Id,
                    ClickedAt = DateTime.UtcNow,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Referrer = Request.Headers["Referer"].ToString()
                };
                await _clickLogRepository.CreateAsync(clickLog);
            }
            catch { }
        });

        return RedirectPermanent(url.OriginalUrl);
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