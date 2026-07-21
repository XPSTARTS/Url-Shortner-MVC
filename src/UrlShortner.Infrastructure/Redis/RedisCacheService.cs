// src/UrlShortner.Infrastructure/Redis/RedisCacheService.cs
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Infrastructure.Redis;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _database;
    private readonly ConnectionMultiplexer _redis;

    public RedisCacheService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? throw new ArgumentNullException("Redis connection string not found");

        _redis = ConnectionMultiplexer.Connect(connectionString);
        _database = _redis.GetDatabase();
    }

    // ============================================
    // URL CACHING
    // ============================================
    public async Task SetUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null)
    {
        var key = $"url:{shortCode}";
        await _database.StringSetAsync(key, originalUrl, expiry ?? TimeSpan.FromHours(24));
    }

    public async Task<string?> GetUrlAsync(string shortCode)
    {
        var key = $"url:{shortCode}";
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveUrlAsync(string shortCode)
    {
        var key = $"url:{shortCode}";
        await _database.KeyDeleteAsync(key);
    }

    // ============================================
    // RATE LIMITING
    // ============================================
    public async Task<bool> IncrementRateLimitAsync(string key, int maxRequests, TimeSpan window)
    {
        var count = await _database.StringIncrementAsync(key);

        if (count == 1)
        {
            // First request - set expiry
            await _database.KeyExpireAsync(key, window);
        }

        return count <= maxRequests;
    }

    // ============================================
    // OTP STORAGE
    // ============================================
    public async Task SetOtpAsync(string key, string hashedOtp, TimeSpan expiry)
    {
        await _database.StringSetAsync(key, hashedOtp, expiry);
    }

    public async Task<string?> GetOtpAsync(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveOtpAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    // ============================================
    // REFRESH TOKEN STORAGE
    // ============================================
    public async Task SetRefreshTokenAsync(string key, string value, TimeSpan expiry)
    {
        await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> RefreshTokenExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveRefreshTokenAsync(string key)
    {
        await _database.KeyDeleteAsync(key);
    }

    // ============================================
    // GENERAL
    // ============================================
    public async Task<bool> KeyExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }
}