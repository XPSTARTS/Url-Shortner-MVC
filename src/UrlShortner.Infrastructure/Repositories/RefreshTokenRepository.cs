// src/UrlShortner.Infrastructure/Repositories/RefreshTokenRepository.cs
using Dapper;
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;

namespace UrlShortner.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(RefreshToken refreshToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            INSERT INTO RefreshTokens (UserId, Token, DeviceInfo, IPAddress, ExpiresAt, CreatedAt)
            VALUES (@UserId, @Token, @DeviceInfo, @IPAddress, @ExpiresAt, @CreatedAt);
            
            SELECT CAST(SCOPE_IDENTITY() as int);";

        return await connection.QuerySingleAsync<int>(sql, refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT Id, UserId, Token, DeviceInfo, IPAddress, ExpiresAt, IsRevoked, CreatedAt, RevokedAt
            FROM RefreshTokens
            WHERE Token = @Token AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()";

        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(sql, new { Token = token });
    }

    public async Task<bool> RevokeAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE RefreshTokens 
            SET IsRevoked = 1, RevokedAt = GETUTCDATE()
            WHERE Token = @Token";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Token = token });
        return rowsAffected > 0;
    }

    public async Task<bool> RevokeAllUserTokensAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            UPDATE RefreshTokens 
            SET IsRevoked = 1, RevokedAt = GETUTCDATE()
            WHERE UserId = @UserId AND IsRevoked = 0";

        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
        return rowsAffected > 0;
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = @"
            SELECT COUNT(1) 
            FROM RefreshTokens 
            WHERE Token = @Token 
            AND IsRevoked = 0 
            AND ExpiresAt > GETUTCDATE()";

        var count = await connection.ExecuteScalarAsync<int>(sql, new { Token = token });
        return count > 0;
    }
}