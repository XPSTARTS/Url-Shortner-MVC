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

        // Validate format - MUST have a valid TLD or be a known domain
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return (false, "Invalid URL format.", null);

        // Must be http or https
        if (uri.Scheme != "http" && uri.Scheme != "https")
            return (false, "Only HTTP and HTTPS URLs are allowed.", null);

        // Must have a valid host with at least one dot (domain.tld)
        var host = uri.Host;
        if (string.IsNullOrEmpty(host))
            return (false, "Invalid URL format.", null);

        // Must contain at least one dot (e.g., example.com) OR be localhost
        if (!host.Contains('.') && host != "localhost")
            return (false, "Invalid URL format.", null);

        // Check for blocked domains
        var hostLower = host.ToLower();
        if (_blockedDomains.Contains(hostLower) || hostLower.EndsWith(".localhost"))
            return (false, "This domain cannot be shortened.", null);

        // Max URL length
        if (url.Length > 2048)
            return (false, "URL is too long. Maximum 2048 characters.", null);

        return (true, null, uri.ToString());
    }
}