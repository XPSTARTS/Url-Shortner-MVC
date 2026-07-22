// src/UrlShortner.Infrastructure/Repositories/UserRepository.cs
using Dapper;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;

namespace UrlShortner.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public UserRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Email, PasswordHash, EmailVerified, CreatedAt, LastLoginAt
            FROM Users
            WHERE Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, Email, PasswordHash, EmailVerified, CreatedAt, LastLoginAt
            FROM Users
            WHERE Email = @Email";

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, EmailVerified, CreatedAt)
            VALUES (@Email, @PasswordHash, @EmailVerified, @CreatedAt);
            
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.QuerySingleAsync<int>(sql, new
        {
            user.Email,
            user.PasswordHash,
            user.EmailVerified,
            user.CreatedAt
        });

        return id;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
        UPDATE Users 
        SET PasswordHash = @PasswordHash
        WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = userId,
            PasswordHash = newPasswordHash
        });

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE Users 
            SET LastLoginAt = GETUTCDATE()
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = userId });
        return rowsAffected > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }
}