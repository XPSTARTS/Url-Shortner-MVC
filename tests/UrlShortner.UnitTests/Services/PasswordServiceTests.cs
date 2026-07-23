// tests/UrlShortner.UnitTests/Services/PasswordServiceTests.cs
using FluentAssertions;
using UrlShortner.Application.Services;

namespace UrlShortner.UnitTests.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _passwordService = new();

    [Fact]
    public void HashPassword_ReturnsDifferentString()
    {
        // Act
        var hash = _passwordService.HashPassword("MyPassword123");

        // Assert
        hash.Should().NotBe("MyPassword123");
        hash.Should().StartWith("$2a$"); // BCrypt hash format
    }

    [Fact]
    public void HashPassword_SamePasswordTwice_ReturnsDifferentHashes()
    {
        // Act
        var hash1 = _passwordService.HashPassword("MyPassword123");
        var hash2 = _passwordService.HashPassword("MyPassword123");

        // Assert (different salts = different hashes)
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hash = _passwordService.HashPassword("MyPassword123");

        // Act
        var result = _passwordService.VerifyPassword("MyPassword123", hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _passwordService.HashPassword("MyPassword123");

        // Act
        var result = _passwordService.VerifyPassword("WrongPassword", hash);

        // Assert
        result.Should().BeFalse();
    }
}