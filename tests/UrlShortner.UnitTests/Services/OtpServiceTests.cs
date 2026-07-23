// tests/UrlShortner.UnitTests/Services/OtpServiceTests.cs
using FluentAssertions;
using Moq;
using UrlShortner.Application.Services;
using UrlShortner.Domain.Enums;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.UnitTests.Services;

public class OtpServiceTests
{
    private readonly Mock<IRedisCacheService> _redisMock = new();
    private readonly OtpService _otpService;

    public OtpServiceTests()
    {
        _otpService = new OtpService(_redisMock.Object);
    }

    [Fact]
    public async Task GenerateOtpAsync_Returns6DigitString()
    {
        // Arrange
        _redisMock.Setup(r => r.SetOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                  .Returns(Task.CompletedTask);

        // Act
        var otp = await _otpService.GenerateOtpAsync("test@test.com", OtpPurpose.Register);

        // Assert
        otp.Should().HaveLength(6);
        otp.All(char.IsDigit).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateOtpAsync_StoresHashInRedis()
    {
        // Arrange
        string? storedKey = null;
        string? storedHash = null;

        _redisMock.Setup(r => r.SetOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                  .Callback<string, string, TimeSpan>((key, hash, _) =>
                  {
                      storedKey = key;
                      storedHash = hash;
                  })
                  .Returns(Task.CompletedTask);

        // Act
        await _otpService.GenerateOtpAsync("test@test.com", OtpPurpose.Register);

        // Assert
        storedKey.Should().Be("otp:register:test@test.com");
        storedHash.Should().NotBeNullOrEmpty();
        storedHash.Should().NotBe("123456"); // Should be hashed, not plain
    }

    [Fact]
    public async Task VerifyOtpAsync_CorrectOtp_ReturnsTrue()
    {
        // Arrange
        var email = "test@test.com";
        var otp = await GenerateAndCaptureOtp(email, OtpPurpose.Register);

        _redisMock.Setup(r => r.GetOtpAsync($"otp:register:{email}"))
                  .ReturnsAsync(HashOtp(otp));
        _redisMock.Setup(r => r.RemoveOtpAsync(It.IsAny<string>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _otpService.VerifyOtpAsync(email, otp, OtpPurpose.Register);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyOtpAsync_WrongOtp_ReturnsFalse()
    {
        // Arrange
        var email = "test@test.com";
        var otp = await GenerateAndCaptureOtp(email, OtpPurpose.Register);

        _redisMock.Setup(r => r.GetOtpAsync($"otp:register:{email}"))
                  .ReturnsAsync(HashOtp(otp));
        _redisMock.Setup(r => r.RemoveOtpAsync(It.IsAny<string>()))
                  .Returns(Task.CompletedTask);

        // Act
        var result = await _otpService.VerifyOtpAsync(email, "000000", OtpPurpose.Register);

        // Assert
        result.Should().BeFalse();
    }

    // Helper methods
    private async Task<string> GenerateAndCaptureOtp(string email, OtpPurpose purpose)
    {
        string? capturedOtp = null;

        _redisMock.Setup(r => r.SetOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                  .Callback<string, string, TimeSpan>((_, hash, __) => { /* Just capture */ })
                  .Returns(Task.CompletedTask);

        return await _otpService.GenerateOtpAsync(email, purpose);
    }

    private string HashOtp(string otp)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(otp);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}