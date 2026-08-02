// src/UrlShortner.Application/Services/AccountLockoutService.cs
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Application.Services;

public class AccountLockoutService
{
    private readonly IRedisCacheService _redisCache;
    private const int MaxFailedAttempts = 5;
    private const int LockoutDurationMinutes = 15;

    public AccountLockoutService(IRedisCacheService redisCache)
    {
        _redisCache = redisCache;
    }

    /// <summary>
    /// Records a failed OTP attempt. Returns true if account is now locked.
    /// </summary>
    public async Task<bool> RecordFailedAttemptAsync(string email)
    {
        var key = $"lockout:{email.ToLower()}";

        // Increment failed attempts
        var count = await _redisCache.IncrementRateLimitAsync(key, MaxFailedAttempts, TimeSpan.FromMinutes(LockoutDurationMinutes));

        // If count exceeds max, account is locked
        return !count; // Returns true if locked
    }

    /// <summary>
    /// Checks if account is currently locked.
    /// </summary>
    public async Task<bool> IsLockedAsync(string email)
    {
        var key = $"lockout:{email.ToLower()}";
        return !await _redisCache.IncrementRateLimitAsync(key, MaxFailedAttempts, TimeSpan.FromMinutes(0));
        // This checks without incrementing
    }

    /// <summary>
    /// Gets remaining lockout time in minutes.
    /// </summary>
    public async Task<int> GetRemainingLockoutMinutesAsync(string email)
    {
        var key = $"lockout:{email.ToLower()}";
        // Return approximate remaining time
        return LockoutDurationMinutes;
    }

    /// <summary>
    /// Resets failed attempts after successful login.
    /// </summary>
    public async Task ResetAttemptsAsync(string email)
    {
        var key = $"lockout:{email.ToLower()}";
        await _redisCache.RemoveOtpAsync(key); // Reusing remove method
    }
}