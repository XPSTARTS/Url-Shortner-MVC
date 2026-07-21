// src/UrlShortner.Application/Services/ShortCodeGenerator.cs
using System.Security.Cryptography;

namespace UrlShortner.Application.Services;

/// <summary>
/// Generates unique short codes for URLs.
/// </summary>
public class ShortCodeGenerator
{
    private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int DefaultCodeLength = 7;

    /// <summary>
    /// Generates a cryptographically secure random short code.
    /// </summary>
    public string GenerateCode(int length = DefaultCodeLength)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Characters[bytes[i] % Characters.Length];
        }

        return new string(chars);
    }

    /// <summary>
    /// Validates a custom alias.
    /// </summary>
    public (bool IsValid, string? Error) ValidateCustomAlias(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            return (false, "Alias cannot be empty.");

        if (alias.Length < 3)
            return (false, "Custom alias must be at least 3 characters.");

        if (alias.Length > 50)
            return (false, "Custom alias must be 50 characters or less.");

        // Only allow alphanumeric, hyphens, and underscores
        if (!System.Text.RegularExpressions.Regex.IsMatch(alias, @"^[a-zA-Z0-9\-_]+$"))
            return (false, "Only letters, numbers, hyphens, and underscores are allowed.");

        // Reserved words
        var reservedWords = new[] { "admin", "login", "register", "dashboard", "api", "health", "auth" };
        if (reservedWords.Contains(alias.ToLower()))
            return (false, "This alias is reserved. Please choose another.");

        return (true, null);
    }
}