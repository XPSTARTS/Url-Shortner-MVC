// src/UrlShortner.Domain/Interfaces/IRedisCacheService.cs
namespace UrlShortner.Domain.Interfaces;

public interface IRedisCacheService
{
    // URL Caching
    Task SetUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null);
    Task<string?> GetUrlAsync(string shortCode);
    Task RemoveUrlAsync(string shortCode);

    // Rate Limiting
    Task<bool> IncrementRateLimitAsync(string key, int maxRequests, TimeSpan window);

    // OTP Storage
    Task SetOtpAsync(string key, string hashedOtp, TimeSpan expiry);
    Task<string?> GetOtpAsync(string key);
    Task RemoveOtpAsync(string key);

    // Refresh Token Storage
    Task SetRefreshTokenAsync(string key, string value, TimeSpan expiry);
    Task<bool> RefreshTokenExistsAsync(string key);
    Task RemoveRefreshTokenAsync(string key);

    // General
    Task<bool> KeyExistsAsync(string key);
}