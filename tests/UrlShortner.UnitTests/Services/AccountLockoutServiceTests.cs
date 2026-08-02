// tests/UrlShortner.UnitTests/Services/AccountLockoutServiceTests.cs
using FluentAssertions;
using Moq;
using UrlShortner.Application.Services;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.UnitTests.Services;

public class AccountLockoutServiceTests
{
    private readonly Mock<IRedisCacheService> _redisMock = new();
    private readonly AccountLockoutService _lockoutService;

    public AccountLockoutServiceTests()
    {
        _lockoutService = new AccountLockoutService(_redisMock.Object);
    }

    [Fact]
    public async Task RecordFailedAttemptAsync_UnderLimit_NotLocked()
    {
        // Arrange - returns true (under limit)
        _redisMock.Setup(r => r.IncrementRateLimitAsync(It.IsAny<string>(), 5, It.IsAny<TimeSpan>()))
                  .ReturnsAsync(true);

        // Act
        var isLocked = await _lockoutService.RecordFailedAttemptAsync("test@test.com");

        // Assert
        isLocked.Should().BeFalse(); // Not locked = returns false
    }

    [Fact]
    public async Task RecordFailedAttemptAsync_ExceedsLimit_Locked()
    {
        // Arrange - returns false (over limit)
        _redisMock.Setup(r => r.IncrementRateLimitAsync(It.IsAny<string>(), 5, It.IsAny<TimeSpan>()))
                  .ReturnsAsync(false);

        // Act
        var isLocked = await _lockoutService.RecordFailedAttemptAsync("test@test.com");

        // Assert
        isLocked.Should().BeTrue(); // Locked = returns true
    }

    [Fact]
    public async Task ResetAttemptsAsync_CallsRedisRemove()
    {
        // Arrange
        _redisMock.Setup(r => r.RemoveOtpAsync(It.IsAny<string>()))
                  .Returns(Task.CompletedTask);

        // Act
        await _lockoutService.ResetAttemptsAsync("test@test.com");

        // Assert
        _redisMock.Verify(r => r.RemoveOtpAsync(It.Is<string>(k => k.StartsWith("lockout:"))), Times.Once);
    }
}