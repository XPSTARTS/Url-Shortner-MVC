// src/UrlShortner.Application/Services/RefreshTokenService.cs
using System.Security.Cryptography;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Application.Services;

/// <summary>
/// Manages refresh token generation, validation, and rotation.
/// </summary>
public class RefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRedisCacheService _redisCache;
    private const int RefreshTokenExpiryDays = 7;

    public RefreshTokenService(
        IRefreshTokenRepository refreshTokenRepository,
        IRedisCacheService redisCache)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _redisCache = redisCache;
    }

    /// <summary>
    /// Generates a new refresh token and stores it in DB + Redis.
    /// </summary>
    public async Task<string> GenerateRefreshTokenAsync(int userId, string? ipAddress = null, string? deviceInfo = null)
    {
        var token = GenerateSecureToken();

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = token,
            DeviceInfo = deviceInfo,
            IPAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        // Store in database (persistent)
        await _refreshTokenRepository.CreateAsync(refreshToken);

        // Store in Redis (fast lookup)
        var redisKey = $"refresh:{userId}:{token}";
        await _redisCache.SetRefreshTokenAsync(redisKey, "valid", TimeSpan.FromDays(RefreshTokenExpiryDays));

        return token;
    }

    /// <summary>
    /// Validates a refresh token (checks Redis first, then DB).
    /// </summary>
    public async Task<bool> ValidateRefreshTokenAsync(int userId, string token)
    {
        // Check Redis first (fast)
        var redisKey = $"refresh:{userId}:{token}";
        if (await _redisCache.RefreshTokenExistsAsync(redisKey))
        {
            return true;
        }

        // Fallback to database
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(token);
        if (storedToken != null && storedToken.UserId == userId)
        {
            // Re-cache in Redis
            var remainingTime = storedToken.ExpiresAt - DateTime.UtcNow;
            if (remainingTime.TotalSeconds > 0)
            {
                await _redisCache.SetRefreshTokenAsync(redisKey, "valid", remainingTime);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// Rotates a refresh token (revoke old, issue new).
    /// </summary>
    public async Task<string?> RotateRefreshTokenAsync(string oldToken, int userId, string? ipAddress = null)
    {
        // Revoke old token
        await RevokeRefreshTokenAsync(userId, oldToken);

        // Generate new token
        return await GenerateRefreshTokenAsync(userId, ipAddress);
    }

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    public async Task RevokeRefreshTokenAsync(int userId, string token)
    {
        await _refreshTokenRepository.RevokeAsync(token);
        var redisKey = $"refresh:{userId}:{token}";
        await _redisCache.RemoveRefreshTokenAsync(redisKey);
    }

    /// <summary>
    /// Revokes all refresh tokens for a user (logout from all devices).
    /// </summary>
    public async Task RevokeAllUserTokensAsync(int userId)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
        // Note: We can't easily remove all Redis keys for a user
        // They will expire naturally
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// </summary>
    private string GenerateSecureToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}