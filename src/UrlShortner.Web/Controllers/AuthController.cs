// src/UrlShortner.Web/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using UrlShortner.Application.Services;
using UrlShortner.Web.Models;

namespace UrlShortner.Web.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(AuthService authService, JwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    // ============================================
    // REGISTER FLOW
    // ============================================

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.InitiateRegistrationAsync(model.Email, model.Password);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        // Store email in TempData to carry to OTP page
        TempData["Email"] = model.Email;
        TempData["Password"] = model.Password;
        TempData["Purpose"] = "Register";
        TempData["Message"] = result.Message;

        return RedirectToAction("VerifyOtp");
    }

    // ============================================
    // LOGIN FLOW
    // ============================================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.InitiateLoginAsync(model.Email, model.Password);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        // Store in TempData for OTP page
        TempData["Email"] = model.Email;
        TempData["Purpose"] = "Login";
        TempData["RememberMe"] = model.RememberMe;
        TempData["ReturnUrl"] = returnUrl;
        TempData["Message"] = result.Message;

        return RedirectToAction("VerifyOtp");
    }

    // ============================================
    // OTP VERIFICATION
    // ============================================

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        var email = TempData["Email"] as string;
        var purpose = TempData["Purpose"] as string;

        if (string.IsNullOrEmpty(email))
            return RedirectToAction("Login");

        // Re-store TempData for the POST
        TempData.Keep("Email");
        TempData.Keep("Purpose");
        TempData.Keep("Password");
        TempData.Keep("RememberMe");
        TempData.Keep("ReturnUrl");

        var model = new VerifyOtpViewModel
        {
            Email = email,
            Purpose = purpose ?? "Login"
        };

        ViewBag.Message = TempData["Message"] as string;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.Purpose == "Register")
        {
            var password = TempData["Password"] as string;
            if (string.IsNullOrEmpty(password))
                return RedirectToAction("Register");

            var result = await _authService.CompleteRegistrationAsync(
                model.Email, password, model.Otp, GetIpAddress());

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                TempData.Keep("Email");
                TempData.Keep("Purpose");
                TempData.Keep("Password");
                return View(model);
            }

            // Set cookies
            SetTokenCookies(result.AccessToken!, result.RefreshToken!);

            TempData["SuccessMessage"] = "Account created successfully! 🎉";
            return RedirectToAction("Index", "Dashboard");
        }
        else // Login
        {
            var result = await _authService.CompleteLoginAsync(
                model.Email, model.Otp, GetIpAddress(), GetUserAgent());

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                TempData.Keep("Email");
                TempData.Keep("Purpose");
                return View(model);
            }

            // Set cookies
            SetTokenCookies(result.AccessToken!, result.RefreshToken!);

            var returnUrl = TempData["ReturnUrl"] as string;
            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }
    }

    // ============================================
    // LOGOUT
    // ============================================

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["RefreshToken"];
        var userId = GetUserIdFromCookie();

        if (!string.IsNullOrEmpty(refreshToken) && userId.HasValue)
        {
            await _authService.LogoutAsync(userId.Value, refreshToken);
        }

        // Clear cookies
        Response.Cookies.Delete("AccessToken");
        Response.Cookies.Delete("RefreshToken");

        return RedirectToAction("Index", "Home");
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private void SetTokenCookies(string accessToken, string refreshToken)
    {
        // Access Token Cookie (short-lived)
        Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddMinutes(15)
        });

        // Refresh Token Cookie (longer-lived)
        Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        });
    }

    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private string GetUserAgent()
    {
        return Request.Headers["User-Agent"].ToString();
    }

    private int? GetUserIdFromCookie()
    {
        var token = Request.Cookies["AccessToken"];
        if (string.IsNullOrEmpty(token)) return null;

        var principal = _jwtTokenService.ValidateToken(token);
        return _jwtTokenService.GetUserId(principal);
    }
}