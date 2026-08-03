// src/UrlShortner.Infrastructure/Repositories/UserRepository.cs
using Dapper;
using Npgsql;
using System.Data;
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

    private bool IsPostgres(IDbConnection connection) => connection is NpgsqlConnection;

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var pg = IsPostgres(connection);
        var sql = pg
            ? @"SELECT ""Id"", ""Email"", ""PasswordHash"", ""FullName"", ""EmailVerified"", ""CreatedAt"", ""LastLoginAt"" FROM ""Users"" WHERE ""Id"" = @Id"
            : @"SELECT Id, Email, PasswordHash, FullName, EmailVerified, CreatedAt, LastLoginAt FROM Users WHERE Id = @Id";

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var pg = IsPostgres(connection);
        var sql = pg
            ? @"SELECT ""Id"", ""Email"", ""PasswordHash"", ""FullName"", ""EmailVerified"", ""CreatedAt"", ""LastLoginAt"" FROM ""Users"" WHERE ""Email"" = @Email"
            : @"SELECT Id, Email, PasswordHash, FullName, EmailVerified, CreatedAt, LastLoginAt FROM Users WHERE Email = @Email";

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        var pg = IsPostgres(connection);
        var sql = pg
            ? @"INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""FullName"", ""EmailVerified"", ""CreatedAt"") VALUES (@Email, @PasswordHash, @FullName, @EmailVerified, @CreatedAt) RETURNING ""Id"";"
            : @"INSERT INTO Users (Email, PasswordHash, FullName, EmailVerified, CreatedAt) VALUES (@Email, @PasswordHash, @FullName, @EmailVerified, @CreatedAt); SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.QuerySingleAsync<int>(sql, new
        {
            user.Email,
            user.PasswordHash,
            user.FullName,
            user.EmailVerified,
            user.CreatedAt
        });
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        using var connection = _connectionFactory.CreateConnection();
        var pg = IsPostgres(connection);
        var sql = pg
            ? @"UPDATE ""Users"" SET ""PasswordHash"" = @PasswordHash WHERE ""Id"" = @Id"
            : @"UPDATE Users SET PasswordHash = @PasswordHash WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = userId, PasswordHash = newPasswordHash });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var pg = IsPostgres(connection);
        var sql = pg
            ? @"UPDATE ""Users"" SET ""LastLoginAt"" = NOW() WHERE ""Id"" = @Id"
            : @"UPDATE Users SET LastLoginAt = GETUTCDATE() WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = userId });
        return rowsAffected > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var pg = IsPostgres(connection);
        var sql = pg
            ? @"SELECT COUNT(1) FROM ""Users"" WHERE ""Email"" = @Email"
            : @"SELECT COUNT(1) FROM Users WHERE Email = @Email";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }
}