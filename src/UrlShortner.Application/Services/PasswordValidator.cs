// src/UrlShortner.Application/Services/PasswordValidator.cs
namespace UrlShortner.Application.Services;

public class PasswordValidator
{
    public (bool IsValid, string? Error) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password is required.");

        if (password.Length < 8)
            return (false, "Password must be at least 8 characters.");

        if (!password.Any(char.IsUpper))
            return (false, "Password must contain at least one uppercase letter.");

        if (!password.Any(char.IsLower))
            return (false, "Password must contain at least one lowercase letter.");

        if (!password.Any(char.IsDigit))
            return (false, "Password must contain at least one number.");

        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            return (false, "Password must contain at least one special character (!@#$%^&*).");

        return (true, null);
    }

    public int GetStrengthScore(string password)
    {
        if (string.IsNullOrEmpty(password)) return 0;

        int score = 0;
        if (password.Length >= 8) score += 20;
        if (password.Length >= 12) score += 10;
        if (password.Any(char.IsUpper)) score += 20;
        if (password.Any(char.IsLower)) score += 20;
        if (password.Any(char.IsDigit)) score += 15;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 15;

        return Math.Min(score, 100);
    }
}