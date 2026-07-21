// src/UrlShortner.Web/Controllers/UrlController.cs
using Microsoft.AspNetCore.Mvc;
using UrlShortner.Application.DTOs;
using UrlShortner.Application.Services;
using UrlShortner.Web.Models;

namespace UrlShortner.Web.Controllers;

public class UrlController : Controller
{
    private readonly UrlShorteningService _urlShorteningService;

    public UrlController(UrlShorteningService urlShorteningService)
    {
        _urlShorteningService = urlShorteningService;
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ShortenUrlViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ShortenUrlViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Get user ID if logged in
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid))
            {
                userId = uid;
            }
        }

        var request = new ShortenUrlRequest
        {
            OriginalUrl = model.OriginalUrl,
            CustomAlias = model.CustomAlias,
            UserId = userId
        };

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = await _urlShorteningService.ShortenUrlAsync(request, baseUrl);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(model);
        }

        // Store result in TempData to show on result page
        TempData["ShortUrl"] = result.ShortUrl;
        TempData["OriginalUrl"] = result.OriginalUrl;
        TempData["ShortCode"] = result.ShortCode;

        return RedirectToAction("Result");
    }

    [HttpGet]
    public IActionResult Result()
    {
        var shortUrl = TempData["ShortUrl"] as string;
        var originalUrl = TempData["OriginalUrl"] as string;
        var shortCode = TempData["ShortCode"] as string;

        if (string.IsNullOrEmpty(shortUrl))
            return RedirectToAction("Create");

        ViewBag.ShortUrl = shortUrl;
        ViewBag.OriginalUrl = originalUrl;
        ViewBag.ShortCode = shortCode;

        return View();
    }
}