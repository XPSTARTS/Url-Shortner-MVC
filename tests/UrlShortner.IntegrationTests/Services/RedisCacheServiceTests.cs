// tests/UrlShortner.IntegrationTests/Services/RedisCacheServiceTests.cs
using FluentAssertions;
using Xunit;
namespace UrlShortner.IntegrationTests.Services;

public class RedisCacheServiceTests : TestBase
{
    [Fact]
    public async Task SetAndGetUrl_Works()
    {
        // Arrange
        var shortCode = $"redis-{Guid.NewGuid():N}"[..10];
        var originalUrl = "https://redis-test.com";

        // Act
        await RedisCache.SetUrlAsync(shortCode, originalUrl, TimeSpan.FromMinutes(1));
        var retrieved = await RedisCache.GetUrlAsync(shortCode);

        // Assert
        retrieved.Should().Be(originalUrl);
    }

    [Fact]
    public async Task GetUrlAsync_NonExistent_ReturnsNull()
    {
        // Act
        var result = await RedisCache.GetUrlAsync("nonexistent-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveUrlAsync_DeletesKey()
    {
        // Arrange
        var shortCode = $"remove-{Guid.NewGuid():N}"[..10];
        await RedisCache.SetUrlAsync(shortCode, "https://remove-test.com");

        // Act
        await RedisCache.RemoveUrlAsync(shortCode);
        var result = await RedisCache.GetUrlAsync(shortCode);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IncrementRateLimitAsync_UnderLimit_ReturnsTrue()
    {
        // Arrange
        var key = $"ratelimit-test-{Guid.NewGuid():N}";

        // Act - First 5 requests should be allowed
        for (int i = 0; i < 5; i++)
        {
            var allowed = await RedisCache.IncrementRateLimitAsync(key, 10, TimeSpan.FromMinutes(1));
            allowed.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SetAndGetOtp_Works()
    {
        // Arrange
        var key = $"otp-test-{Guid.NewGuid():N}";
        var hashedOtp = "test-hash-value";

        // Act
        await RedisCache.SetOtpAsync(key, hashedOtp, TimeSpan.FromMinutes(1));
        var retrieved = await RedisCache.GetOtpAsync(key);

        // Assert
        retrieved.Should().Be(hashedOtp);
    }
}