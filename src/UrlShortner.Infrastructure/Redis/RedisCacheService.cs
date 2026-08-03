// src/UrlShortner.Infrastructure/Redis/RedisCacheService.cs
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Infrastructure.Redis;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase? _database;
    private readonly ConnectionMultiplexer? _redis;
    private readonly bool _isAvailable;

    public RedisCacheService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrEmpty(connectionString) || connectionString == "localhost:6379")
        {
            _isAvailable = false;
            return;
        }

        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
            _isAvailable = true;
        }
        catch
        {
            _isAvailable = false;
        }
    }

    public async Task SetUrlAsync(string shortCode, string originalUrl, TimeSpan? expiry = null)
    {
        if (!_isAvailable || _database == null) return;
        var key = $"url:{shortCode}";
        await _database.StringSetAsync(key, originalUrl, expiry ?? TimeSpan.FromHours(24));
    }

    public async Task<string?> GetUrlAsync(string shortCode)
    {
        if (!_isAvailable || _database == null) return null;
        var key = $"url:{shortCode}";
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveUrlAsync(string shortCode)
    {
        if (!_isAvailable || _database == null) return;
        var key = $"url:{shortCode}";
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> IncrementRateLimitAsync(string key, int maxRequests, TimeSpan window)
    {
        if (!_isAvailable || _database == null) return true; // Allow if no Redis
        var count = await _database.StringIncrementAsync(key);
        if (count == 1) await _database.KeyExpireAsync(key, window);
        return count <= maxRequests;
    }

    public async Task SetOtpAsync(string key, string hashedOtp, TimeSpan expiry)
    {
        if (!_isAvailable || _database == null) return;
        await _database.StringSetAsync(key, hashedOtp, expiry);
    }

    public async Task<string?> GetOtpAsync(string key)
    {
        if (!_isAvailable || _database == null) return null;
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveOtpAsync(string key)
    {
        if (!_isAvailable || _database == null) return;
        await _database.KeyDeleteAsync(key);
    }

    public async Task SetRefreshTokenAsync(string key, string value, TimeSpan expiry)
    {
        if (!_isAvailable || _database == null) return;
        await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<bool> RefreshTokenExistsAsync(string key)
    {
        if (!_isAvailable || _database == null) return false;
        return await _database.KeyExistsAsync(key);
    }

    public async Task RemoveRefreshTokenAsync(string key)
    {
        if (!_isAvailable || _database == null) return;
        await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        if (!_isAvailable || _database == null) return false;
        return await _database.KeyExistsAsync(key);
    }
}