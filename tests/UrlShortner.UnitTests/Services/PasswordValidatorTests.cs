// tests/UrlShortner.UnitTests/Services/PasswordValidatorTests.cs
using FluentAssertions;
using UrlShortner.Application.Services;

namespace UrlShortner.UnitTests.Services;

public class PasswordValidatorTests
{
    private readonly PasswordValidator _validator = new();

    [Theory]
    [InlineData("Weak1@", false)]        // Too short
    [InlineData("weakpassword1@", false)] // No uppercase
    [InlineData("WEAKPASSWORD1@", false)] // No lowercase
    [InlineData("WeakPassword@", false)]  // No number
    [InlineData("WeakPassword1", false)]  // No special char
    [InlineData("StrongPass1@", true)]    // Valid
    [InlineData("Str0ng!Pass", true)]     // Valid
    public void Validate_VariousPasswords_ReturnsExpected(string password, bool expectedValid)
    {
        var (isValid, _) = _validator.Validate(password);
        isValid.Should().Be(expectedValid);
    }

    [Fact]
    public void GetStrengthScore_WeakPassword_ReturnsLowScore()
    {
        var score = _validator.GetStrengthScore("weak");
        score.Should().BeLessThan(50);
    }

    [Fact]
    public void GetStrengthScore_StrongPassword_ReturnsHighScore()
    {
        var score = _validator.GetStrengthScore("Str0ng!PassWord");
        score.Should().BeGreaterThanOrEqualTo(80);
    }

    [Fact]
    public void GetStrengthScore_EmptyPassword_ReturnsZero()
    {
        var score = _validator.GetStrengthScore("");
        score.Should().Be(0);
    }
}