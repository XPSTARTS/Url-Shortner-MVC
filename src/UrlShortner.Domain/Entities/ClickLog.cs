// src/UrlShortner.Domain/Entities/ClickLog.cs
namespace UrlShortner.Domain.Entities;

public class ClickLog
{
    public long Id { get; set; }
    public int ShortUrlId { get; set; }
    public DateTime ClickedAt { get; set; }
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }

    // Navigation property
    public ShortUrl? ShortUrl { get; set; }
}