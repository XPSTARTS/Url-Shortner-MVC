// src/UrlShortner.Infrastructure/Repositories/ClickLogRepository.cs
using Dapper;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;

namespace UrlShortner.Infrastructure.Repositories;

public class ClickLogRepository : IClickLogRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public ClickLogRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(ClickLog clickLog)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO ClickLogs (ShortUrlId, ClickedAt, IPAddress, UserAgent, Referrer)
            VALUES (@ShortUrlId, @ClickedAt, @IPAddress, @UserAgent, @Referrer)";

        await connection.ExecuteAsync(sql, clickLog);
    }

    public async Task<IEnumerable<ClickLog>> GetByShortUrlIdAsync(int shortUrlId, int limit = 100)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT TOP (@Limit) Id, ShortUrlId, ClickedAt, IPAddress, UserAgent, Referrer
            FROM ClickLogs
            WHERE ShortUrlId = @ShortUrlId
            ORDER BY ClickedAt DESC";

        return await connection.QueryAsync<ClickLog>(sql, new { ShortUrlId = shortUrlId, Limit = limit });
    }

    public async Task<int> GetClickCountByDateRangeAsync(int shortUrlId, DateTime start, DateTime end)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(1) 
            FROM ClickLogs 
            WHERE ShortUrlId = @ShortUrlId 
            AND ClickedAt >= @Start 
            AND ClickedAt <= @End";

        return await connection.ExecuteScalarAsync<int>(sql, new { ShortUrlId = shortUrlId, Start = start, End = end });
    }
}