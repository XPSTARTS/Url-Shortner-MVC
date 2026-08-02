// src/UrlShortner.Application/DTOs/ShortenUrlResponse.cs
namespace UrlShortner.Application.DTOs;

public class ShortenUrlResponse
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public string? ShortCode { get; set; }
    public string? ShortUrl { get; set; }
    public string? OriginalUrl { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public static ShortenUrlResponse Success(string shortCode, string shortUrl, string originalUrl, DateTime? expiresAt = null)
    {
        return new ShortenUrlResponse
        {
            IsSuccess = true,
            ShortCode = shortCode,
            ShortUrl = shortUrl,
            OriginalUrl = originalUrl,
            ExpiresAt = expiresAt
        };
    }

    public static ShortenUrlResponse Failure(string error)
    {
        return new ShortenUrlResponse
        {
            IsSuccess = false,
            Error = error
        };
    }
}