// src/UrlShortner.Domain/Entities/ShortUrl.cs
namespace UrlShortner.Domain.Entities;

public class ShortUrl
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long ClickCount { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties (not used by Dapper, but good for documentation)
    public User? User { get; set; }
    public List<ClickLog> ClickLogs { get; set; } = new();
}