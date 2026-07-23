// tests/UrlShortner.IntegrationTests/TestBase.cs
using Microsoft.Extensions.Configuration;
using UrlShortner.Infrastructure.Data;
using UrlShortner.Infrastructure.Redis;
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.IntegrationTests;

/// <summary>
/// Base class for integration tests. Sets up real database and Redis connections.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly IConfiguration Configuration;
    protected readonly DbConnectionFactory DbConnectionFactory;
    protected readonly IRedisCacheService RedisCache;

    protected TestBase()
    {
        // Load test configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: false)
            .Build();

        // Create real connections
        DbConnectionFactory = new DbConnectionFactory(Configuration);
        RedisCache = new RedisCacheService(Configuration);
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}