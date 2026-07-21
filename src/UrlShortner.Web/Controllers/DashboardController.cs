// src/UrlShortner.Web/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Mvc;
using UrlShortner.Domain.Interfaces;
using System.Security.Claims;

namespace UrlShortner.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IShortUrlRepository _shortUrlRepository;
    private readonly IUserRepository _userRepository;

    public DashboardController(
        IShortUrlRepository shortUrlRepository,
        IUserRepository userRepository)
    {
        _shortUrlRepository = shortUrlRepository;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login", "Auth");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return RedirectToAction("Login", "Auth");

        // Get user's URLs
        var urls = await _shortUrlRepository.GetByUserIdAsync(userId);
        var urlList = urls.ToList();

        // Calculate stats
        ViewBag.TotalUrls = urlList.Count;
        ViewBag.TotalClicks = urlList.Sum(u => (long)u.ClickCount);
        ViewBag.ActiveUrls = urlList.Count(u => u.IsActive);
        ViewBag.UserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        ViewBag.UserId = userId;

        return View(urlList);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login", "Auth");

        await _shortUrlRepository.SoftDeleteAsync(id);
        TempData["Message"] = "URL deleted successfully.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return RedirectToAction("Login", "Auth");

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return RedirectToAction("Login", "Auth");

        var user = await _userRepository.GetByIdAsync(userId);
        var urls = await _shortUrlRepository.GetByUserIdAsync(userId);
        var urlList = urls.ToList();

        ViewBag.TotalUrls = urlList.Count;
        ViewBag.TotalClicks = urlList.Sum(u => (long)u.ClickCount);
        ViewBag.ActiveUrls = urlList.Count(u => u.IsActive);

        return View(user);
    }
}