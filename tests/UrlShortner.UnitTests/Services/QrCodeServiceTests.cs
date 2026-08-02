// tests/UrlShortner.UnitTests/Services/QrCodeServiceTests.cs
using FluentAssertions;
using UrlShortner.Application.Services;

namespace UrlShortner.UnitTests.Services;

public class QrCodeServiceTests
{
    private readonly QrCodeService _qrService = new();

    [Fact]
    public void GenerateQrCodeBase64_ValidUrl_ReturnsBase64String()
    {
        // Act
        var result = _qrService.GenerateQrCodeBase64("https://example.com");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().BeOfType<string>();
    }

    [Fact]
    public void GenerateQrCodeBase64_ReturnsValidBase64()
    {
        // Act
        var result = _qrService.GenerateQrCodeBase64("https://test.com");

        // Assert - Base64 string should decode without error
        var bytes = Convert.FromBase64String(result);
        bytes.Should().NotBeNull();
        bytes.Length.Should().BeGreaterThan(0); // PNG header at minimum
    }

    [Fact]
    public void GenerateQrCodeBase64_DifferentUrls_ProduceDifferentResults()
    {
        // Act
        var result1 = _qrService.GenerateQrCodeBase64("https://url1.com");
        var result2 = _qrService.GenerateQrCodeBase64("https://url2.com");

        // Assert
        result1.Should().NotBe(result2);
    }
}