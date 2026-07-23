// tests/UrlShortner.IntegrationTests/Repositories/UserRepositoryTests.cs
using FluentAssertions;
using UrlShortner.Domain.Entities;
using UrlShortner.Infrastructure.Repositories;
using Xunit;

namespace UrlShortner.IntegrationTests.Repositories;

public class UserRepositoryTests : TestBase
{
    private readonly UserRepository _userRepository;

    public UserRepositoryTests()
    {
        _userRepository = new UserRepository(DbConnectionFactory);
    }

    [Fact]
    public async Task CreateAndGetUser_CompleteFlow_Works()
    {
        // Arrange
        var uniqueEmail = $"test-{Guid.NewGuid():N}@integration.com";
        var user = new User
        {
            Email = uniqueEmail,
            PasswordHash = "hashed-password",
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act - Create
        var userId = await _userRepository.CreateAsync(user);

        // Assert - Create
        userId.Should().BeGreaterThan(0);

        // Act - Get by Email
        var retrievedUser = await _userRepository.GetByEmailAsync(uniqueEmail);

        // Assert - Get by Email
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Should().Be(uniqueEmail);
        retrievedUser.Id.Should().Be(userId);

        // Act - Get by Id
        var userById = await _userRepository.GetByIdAsync(userId);

        // Assert - Get by Id
        userById.Should().NotBeNull();
        userById!.Email.Should().Be(uniqueEmail);

        // Act - Check email exists
        var exists = await _userRepository.EmailExistsAsync(uniqueEmail);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
    {
        // Act
        var exists = await _userRepository.EmailExistsAsync($"nonexistent-{Guid.NewGuid():N}@test.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePasswordAsync_ValidUser_UpdatesPassword()
    {
        // Arrange - Create a user first
        var email = $"pwdtest-{Guid.NewGuid():N}@integration.com";
        var user = new User
        {
            Email = email,
            PasswordHash = "old-hash",
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };
        var userId = await _userRepository.CreateAsync(user);

        // Act
        var updated = await _userRepository.UpdatePasswordAsync(userId, "new-hash");

        // Assert
        updated.Should().BeTrue();

        // Verify
        var retrievedUser = await _userRepository.GetByIdAsync(userId);
        retrievedUser!.PasswordHash.Should().Be("new-hash");
    }
}