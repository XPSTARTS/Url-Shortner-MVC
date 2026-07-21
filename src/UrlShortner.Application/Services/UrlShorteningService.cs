// src/UrlShortner.Application/Services/UrlShorteningService.cs
using UrlShortner.Application.DTOs;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace UrlShortner.Application.Services;

/// <summary>
/// Orchestrates URL shortening logic.
/// </summary>
public class UrlShorteningService
{
    private readonly IShortUrlRepository _shortUrlRepository;
    private readonly IRedisCacheService _redisCache;
    private readonly ShortCodeGenerator _codeGenerator;
    private readonly UrlValidator _urlValidator;
    private readonly ILogger<UrlShorteningService>? _logger;

    public UrlShorteningService(
        IShortUrlRepository shortUrlRepository,
        IRedisCacheService redisCache,
        ShortCodeGenerator codeGenerator,
        UrlValidator urlValidator,
        ILogger<UrlShorteningService>? logger = null)
    {
        _shortUrlRepository = shortUrlRepository;
        _redisCache = redisCache;
        _codeGenerator = codeGenerator;
        _urlValidator = urlValidator;
        _logger = logger;
    }

    /// <summary>
    /// Shortens a URL. Uses custom alias if provided, otherwise generates one.
    /// </summary>
    public async Task<ShortenUrlResponse> ShortenUrlAsync(ShortenUrlRequest request, string baseUrl)
    {
        // Step 1: Validate the URL
        var (isValid, error, normalizedUrl) = _urlValidator.ValidateUrl(request.OriginalUrl);
        if (!isValid)
            return ShortenUrlResponse.Failure(error!);

        // Step 2: Determine short code
        string shortCode;

        if (!string.IsNullOrWhiteSpace(request.CustomAlias))
        {
            // Validate custom alias
            var (isAliasValid, aliasError) = _codeGenerator.ValidateCustomAlias(request.CustomAlias);
            if (!isAliasValid)
                return ShortenUrlResponse.Failure(aliasError!);

            // Check if custom alias is available
            if (!await _shortUrlRepository.IsCodeUniqueAsync(request.CustomAlias))
                return ShortenUrlResponse.Failure("This custom alias is already taken. Please choose another.");

            shortCode = request.CustomAlias;
        }
        else
        {
            // Generate unique short code
            shortCode = await GenerateUniqueCodeAsync();
        }

        // Step 3: Create ShortUrl entity
        var shortUrl = new ShortUrl
        {
            UserId = request.UserId,
            OriginalUrl = normalizedUrl!,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0,
            IsActive = true
        };

        // Step 4: Save to database
        var id = await _shortUrlRepository.CreateAsync(shortUrl);

        // Step 5: Cache in Redis for fast redirects
        await _redisCache.SetUrlAsync(shortCode, normalizedUrl!, TimeSpan.FromHours(24));

        _logger?.LogInformation("URL shortened: {ShortCode} -> {OriginalUrl}", shortCode, normalizedUrl);

        // Step 6: Return response
        var shortUrl_full = $"{baseUrl.TrimEnd('/')}/{shortCode}";
        return ShortenUrlResponse.Success(shortCode, shortUrl_full, normalizedUrl!);
    }

    /// <summary>
    /// Gets the original URL for a short code (checks Redis first, then DB).
    /// </summary>
    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        // Check Redis cache first (fast)
        var cachedUrl = await _redisCache.GetUrlAsync(shortCode);
        if (cachedUrl != null)
        {
            _logger?.LogDebug("URL cache hit for: {ShortCode}", shortCode);
            return cachedUrl;
        }

        // Fallback to database
        var shortUrl = await _shortUrlRepository.GetByCodeAsync(shortCode);
        if (shortUrl == null)
            return null;

        // Cache for next time
        await _redisCache.SetUrlAsync(shortCode, shortUrl.OriginalUrl, TimeSpan.FromHours(24));

        return shortUrl.OriginalUrl;
    }

    /// <summary>
    /// Records a click on a short URL.
    /// </summary>
    public async Task RecordClickAsync(string shortCode, string? ipAddress, string? userAgent, string? referrer)
    {
        var shortUrl = await _shortUrlRepository.GetByCodeAsync(shortCode);
        if (shortUrl == null) return;

        await _shortUrlRepository.IncrementClickCountAsync(shortUrl.Id);
    }

    /// <summary>
    /// Generates a guaranteed unique short code.
    /// </summary>
    private async Task<string> GenerateUniqueCodeAsync()
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            var code = _codeGenerator.GenerateCode();

            if (await _shortUrlRepository.IsCodeUniqueAsync(code))
                return code;
        }

        // If we've had collisions, make a longer code
        var longCode = _codeGenerator.GenerateCode(10);
        return longCode;
    }
}