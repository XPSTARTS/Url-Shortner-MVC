// src/UrlShortner.Infrastructure/Repositories/ShortUrlRepository.cs
using Dapper;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;

namespace UrlShortner.Infrastructure.Repositories;

public class ShortUrlRepository : IShortUrlRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ShortUrlRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ShortUrl?> GetByCodeAsync(string shortCode)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, UserId, OriginalUrl, ShortCode, CreatedAt, ClickCount, IsActive
            FROM ShortUrls
            WHERE ShortCode = @ShortCode AND IsActive = 1";

        return await connection.QuerySingleOrDefaultAsync<ShortUrl>(sql, new { ShortCode = shortCode });
    }

    public async Task<IEnumerable<ShortUrl>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, UserId, OriginalUrl, ShortCode, CreatedAt, ClickCount, IsActive
            FROM ShortUrls
            WHERE UserId = @UserId AND IsActive = 1
            ORDER BY CreatedAt DESC";

        return await connection.QueryAsync<ShortUrl>(sql, new { UserId = userId });
    }

    public async Task<int> CreateAsync(ShortUrl shortUrl)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO ShortUrls (UserId, OriginalUrl, ShortCode, CreatedAt, ClickCount, IsActive)
            VALUES (@UserId, @OriginalUrl, @ShortCode, @CreatedAt, @ClickCount, @IsActive);
            
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.QuerySingleAsync<int>(sql, shortUrl);
    }

    public async Task<bool> UpdateAsync(ShortUrl shortUrl)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE ShortUrls 
            SET OriginalUrl = @OriginalUrl, ShortCode = @ShortCode
            WHERE Id = @Id AND IsActive = 1";

        var rows = await connection.ExecuteAsync(sql, shortUrl);
        return rows > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "UPDATE ShortUrls SET IsActive = 0 WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<bool> IncrementClickCountAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "UPDATE ShortUrls SET ClickCount = ClickCount + 1 WHERE Id = @Id";

        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<bool> IsCodeUniqueAsync(string shortCode)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = "SELECT COUNT(1) FROM ShortUrls WHERE ShortCode = @ShortCode";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { ShortCode = shortCode });
        return count == 0;
    }
}