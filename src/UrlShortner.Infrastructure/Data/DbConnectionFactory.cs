// src/UrlShortner.Infrastructure/Data/DbConnectionFactory.cs
using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace UrlShortner.Infrastructure.Data;

public class DbConnectionFactory
{
    private readonly string _connectionString;
    private readonly bool _isPostgres;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("DefaultConnection string not found");

        _isPostgres = _connectionString.Contains("postgresql") ||
                      _connectionString.Contains("supabase") ||
                      _connectionString.Contains("Host=");
    }

    public IDbConnection CreateConnection()
    {
        if (_isPostgres)
        {
            return new NpgsqlConnection(_connectionString);
        }

        return new SqlConnection(_connectionString);
    }
}