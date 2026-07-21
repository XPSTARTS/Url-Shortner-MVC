// src/UrlShortner.Application/Services/UrlValidator.cs
namespace UrlShortner.Application.Services;

/// <summary>
/// Validates URLs before shortening.
/// </summary>
public class UrlValidator
{
    private readonly string[] _blockedDomains = new[]
    {
        "localhost",
        "127.0.0.1",
        "urlshortner.com" // Replace with your actual domain
    };

    /// <summary>
    /// Validates a URL for shortening.
    /// </summary>
    public (bool IsValid, string? Error, string? NormalizedUrl) ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return (false, "URL cannot be empty.", null);

        // Add https:// if no protocol specified
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        // Validate format
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return (false, "Invalid URL format.", null);

        // Must be http or https
        if (uri.Scheme != "http" && uri.Scheme != "https")
            return (false, "Only HTTP and HTTPS URLs are allowed.", null);

        // Check for blocked domains (prevent shortening our own URLs)
        var host = uri.Host.ToLower();
        if (_blockedDomains.Contains(host) || host.EndsWith(".localhost"))
            return (false, "This domain cannot be shortened.", null);

        // Max URL length
        if (url.Length > 2048)
            return (false, "URL is too long. Maximum 2048 characters.", null);

        return (true, null, uri.ToString());
    }
}