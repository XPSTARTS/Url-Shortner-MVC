// tests/UrlShortner.UnitTests/Services/ShortCodeGeneratorTests.cs
using FluentAssertions;
using UrlShortner.Application.Services;

namespace UrlShortner.UnitTests.Services;

public class ShortCodeGeneratorTests
{
    private readonly ShortCodeGenerator _generator = new();

    [Fact]
    public void GenerateCode_Default_Returns7Characters()
    {
        // Act
        var code = _generator.GenerateCode();

        // Assert
        code.Should().HaveLength(7);
    }

    [Fact]
    public void GenerateCode_CustomLength_ReturnsCorrectLength()
    {
        // Act
        var code = _generator.GenerateCode(10);

        // Assert
        code.Should().HaveLength(10);
    }

    [Fact]
    public void GenerateCode_OnlyContainsValidCharacters()
    {
        // Arrange
        var validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Act
        var code = _generator.GenerateCode();

        // Assert
        code.All(c => validChars.Contains(c)).Should().BeTrue();
    }

    [Fact]
    public void GenerateCode_TwoCodesAreDifferent()
    {
        // Act
        var code1 = _generator.GenerateCode();
        var code2 = _generator.GenerateCode();

        // Assert
        code1.Should().NotBe(code2);
    }

    [Theory]
    [InlineData("my-link", true)]
    [InlineData("ab", false)]           // Too short
    [InlineData("", false)]             // Empty
    [InlineData("my link", false)]      // Contains space
    [InlineData("admin", false)]        // Reserved word
    [InlineData("login", false)]        // Reserved word
    [InlineData("dashboard", false)]    // Reserved word
    public void ValidateCustomAlias_VariousInputs_ReturnsExpected(string alias, bool expectedValid)
    {
        // Act
        var (isValid, _) = _generator.ValidateCustomAlias(alias);

        // Assert
        isValid.Should().Be(expectedValid);
    }
}