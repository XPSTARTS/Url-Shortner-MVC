// src/UrlShortner.Application/Services/OtpService.cs
using System.Security.Cryptography;
using UrlShortner.Domain.Enums;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Application.Services;

/// <summary>
/// Handles OTP generation, hashing, and verification.
/// Uses Redis for storage with automatic expiry.
/// </summary>
public class OtpService
{
    private readonly IRedisCacheService _redisCache;
    private const int OtpLength = 6;
    private const int OtpExpiryMinutes = 10;

    public OtpService(IRedisCacheService redisCache)
    {
        _redisCache = redisCache;
    }

    /// <summary>
    /// Generates a 6-digit OTP and stores the hash in Redis.
    /// Returns the plain OTP (to send via email).
    /// </summary>
    public async Task<string> GenerateOtpAsync(string email, OtpPurpose purpose)
    {
        // Generate cryptographically secure 6-digit OTP
        var otp = GenerateSecureOtp();

        // Hash the OTP before storing
        var otpHash = HashOtp(otp);

        // Store in Redis with expiry
        var key = GetOtpKey(email, purpose);
        await _redisCache.SetOtpAsync(key, otpHash, TimeSpan.FromMinutes(OtpExpiryMinutes));

        // Return plain OTP for email
        return otp;
    }

    /// <summary>
    /// Verifies an OTP against the stored hash.
    /// </summary>
    public async Task<bool> VerifyOtpAsync(string email, string otp, OtpPurpose purpose)
    {
        var key = GetOtpKey(email, purpose);
        var storedHash = await _redisCache.GetOtpAsync(key);

        if (string.IsNullOrEmpty(storedHash))
            return false; // Expired or doesn't exist

        var isValid = VerifyOtpHash(otp, storedHash);

        if (isValid)
        {
            // One-time use - remove after verification
            await _redisCache.RemoveOtpAsync(key);
        }

        return isValid;
    }

    /// <summary>
    /// Checks if an OTP exists (hasn't expired).
    /// </summary>
    public async Task<bool> OtpExistsAsync(string email, OtpPurpose purpose)
    {
        var key = GetOtpKey(email, purpose);
        return await _redisCache.KeyExistsAsync(key);
    }

    /// <summary>
    /// Generates a cryptographically secure random OTP.
    /// </summary>
    private string GenerateSecureOtp()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % 1000000;
        return number.ToString("D6"); // Padded to 6 digits
    }

    /// <summary>
    /// Hashes OTP using SHA256.
    /// </summary>
    private string HashOtp(string otp)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(otp);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verifies an OTP against a SHA256 hash.
    /// </summary>
    private bool VerifyOtpHash(string otp, string hash)
    {
        var computedHash = HashOtp(otp);
        return computedHash == hash;
    }

    /// <summary>
    /// Creates a consistent Redis key for OTP.
    /// </summary>
    private string GetOtpKey(string email, OtpPurpose purpose)
    {
        return $"otp:{purpose.ToString().ToLower()}:{email.ToLower()}";
    }
}