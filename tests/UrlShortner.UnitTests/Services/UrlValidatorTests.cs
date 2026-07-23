// tests/UrlShortner.UnitTests/Services/UrlValidatorTests.cs
using FluentAssertions;
using UrlShortner.Application.Services;

namespace UrlShortner.UnitTests.Services;

public class UrlValidatorTests
{
    private readonly UrlValidator _validator = new();

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("http://example.com", true)]
    [InlineData("example.com", true)]
    [InlineData("github.com", true)]
    [InlineData("www.google.com", true)]
    [InlineData("", false)]
    [InlineData("not-a-url", false)]
    [InlineData("justtext", false)]
    [InlineData("ftp://example.com", false)]
    [InlineData("http://localhost", false)]
    [InlineData("http://127.0.0.1", false)]
    [InlineData("http://localhost:5000", false)]
    public void ValidateUrl_VariousInputs_ReturnsExpected(string url, bool expectedValid)
    {
        // Act
        var (isValid, _, _) = _validator.ValidateUrl(url);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    [Fact]
    public void ValidateUrl_NoProtocol_AddsHttps()
    {
        // Act
        var (_, _, normalizedUrl) = _validator.ValidateUrl("github.com");

        // Assert
        normalizedUrl.Should().Be("https://github.com/");
    }
}