// src/UrlShortner.Web/Controllers/HealthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;

namespace UrlShortner.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public HealthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var health = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Services = new
            {
                Database = await CheckDatabaseAsync(),
                Redis = await CheckRedisAsync()
            }
        };

        return Ok(health);
    }

    private async Task<string> CheckDatabaseAsync()
    {
        try
        {
            using var connection = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            return "Connected ✅";
        }
        catch (Exception ex)
        {
            return $"Failed ❌: {ex.Message}";
        }
    }

    private async Task<string> CheckRedisAsync()
    {
        try
        {
            var redis = await ConnectionMultiplexer.ConnectAsync(
                _configuration.GetConnectionString("Redis"));
            var db = redis.GetDatabase();
            await db.PingAsync();
            return "Connected ✅";
        }
        catch (Exception ex)
        {
            return $"Failed ❌: {ex.Message}";
        }
    }
}