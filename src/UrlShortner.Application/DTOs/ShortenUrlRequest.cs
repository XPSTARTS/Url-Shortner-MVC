// src/UrlShortner.Application/DTOs/ShortenUrlRequest.cs
namespace UrlShortner.Application.DTOs;

public class ShortenUrlRequest
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string? CustomAlias { get; set; }
    public int? UserId { get; set; } // Null for anonymous
}