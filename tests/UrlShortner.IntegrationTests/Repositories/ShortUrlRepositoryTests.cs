// tests/UrlShortner.IntegrationTests/Repositories/ShortUrlRepositoryTests.cs
using FluentAssertions;
using UrlShortner.Domain.Entities;
using Xunit;
using UrlShortner.Infrastructure.Repositories;

namespace UrlShortner.IntegrationTests.Repositories;

public class ShortUrlRepositoryTests : TestBase
{
    private readonly ShortUrlRepository _repository;

    public ShortUrlRepositoryTests()
    {
        _repository = new ShortUrlRepository(DbConnectionFactory);
    }

    [Fact]
    public async Task CreateAndGetShortUrl_CompleteFlow_Works()
    {
        // Arrange
        var shortCode = $"test-{Guid.NewGuid():N}"[..10];
        var shortUrl = new ShortUrl
        {
            OriginalUrl = "https://integration-test.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0,
            IsActive = true
        };

        // Act - Create
        var id = await _repository.CreateAsync(shortUrl);
        id.Should().BeGreaterThan(0);

        // Act - Get by Code
        var retrieved = await _repository.GetByCodeAsync(shortCode);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.OriginalUrl.Should().Be("https://integration-test.com");
        retrieved.ShortCode.Should().Be(shortCode);
        retrieved.ClickCount.Should().Be(0);
    }

    [Fact]
    public async Task IncrementClickCount_Works()
    {
        // Arrange
        var shortCode = $"click-{Guid.NewGuid():N}"[..10];
        var shortUrl = new ShortUrl
        {
            OriginalUrl = "https://click-test.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0,
            IsActive = true
        };
        var id = await _repository.CreateAsync(shortUrl);

        // Act
        await _repository.IncrementClickCountAsync(id);
        await _repository.IncrementClickCountAsync(id);
        await _repository.IncrementClickCountAsync(id);

        // Assert
        var retrieved = await _repository.GetByCodeAsync(shortCode);
        retrieved!.ClickCount.Should().Be(3);
    }

    [Fact]
    public async Task IsCodeUniqueAsync_ExistingCode_ReturnsFalse()
    {
        // Arrange
        var shortCode = $"unique-{Guid.NewGuid():N}"[..10];
        var shortUrl = new ShortUrl
        {
            OriginalUrl = "https://unique-test.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.CreateAsync(shortUrl);

        // Act
        var isUnique = await _repository.IsCodeUniqueAsync(shortCode);

        // Assert
        isUnique.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDeleteAsync_MarksUrlInactive()
    {
        // Arrange
        var shortCode = $"delete-{Guid.NewGuid():N}"[..10];
        var shortUrl = new ShortUrl
        {
            OriginalUrl = "https://delete-test.com",
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        var id = await _repository.CreateAsync(shortUrl);

        // Act
        var deleted = await _repository.SoftDeleteAsync(id);

        // Assert
        deleted.Should().BeTrue();

        // Verify - GetByCode should not return inactive URLs
        var retrieved = await _repository.GetByCodeAsync(shortCode);
        retrieved.Should().BeNull(); // Soft deleted, so not found
    }
}