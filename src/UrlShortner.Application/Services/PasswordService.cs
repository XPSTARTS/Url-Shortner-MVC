// src/UrlShortner.Application/Services/PasswordService.cs
namespace UrlShortner.Application.Services;

/// <summary>
/// Handles password hashing and verification using BCrypt.
/// </summary>
public class PasswordService
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    /// <summary>
    /// Verifies a plain text password against a hash.
    /// </summary>
    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}