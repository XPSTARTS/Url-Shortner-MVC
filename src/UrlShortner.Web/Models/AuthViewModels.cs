// src/UrlShortner.Web/Models/AuthViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace UrlShortner.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}

public class VerifyOtpViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    [Display(Name = "Verification Code")]
    public string Otp { get; set; } = string.Empty;

    public string Purpose { get; set; } = string.Empty; // "Register" or "Login"

    // Hidden fields to carry through the flow
    public string? Password { get; set; } // Only for registration
    public bool RememberMe { get; set; }
}

// Add to src/UrlShortner.Web/Models/AuthViewModels.cs (at the bottom)

public class ShortenUrlViewModel
{
    [Required(ErrorMessage = "Please enter a URL")]
    [Url(ErrorMessage = "Please enter a valid URL (include http:// or https://)")]
    [Display(Name = "Long URL")]
    public string OriginalUrl { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Custom alias must be 50 characters or less")]
    [RegularExpression(@"^[a-zA-Z0-9\-_]*$", ErrorMessage = "Only letters, numbers, hyphens, and underscores")]
    [Display(Name = "Custom Alias (optional)")]
    public string? CustomAlias { get; set; }
}