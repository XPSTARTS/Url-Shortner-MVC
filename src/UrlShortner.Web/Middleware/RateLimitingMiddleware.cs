// src/UrlShortner.Web/Middleware/RateLimitingMiddleware.cs
using UrlShortner.Domain.Interfaces;

namespace UrlShortner.Web.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IRedisCacheService redisCache)
    {
        // Only rate limit the URL shortening endpoint
        if (context.Request.Path.StartsWithSegments("/Urls/Create") &&
            context.Request.Method == "POST")
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"ratelimit:create:{ipAddress}";

            var isAllowed = await redisCache.IncrementRateLimitAsync(key, maxRequests: 10, window: TimeSpan.FromMinutes(1));

            if (!isAllowed)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Too many requests. Please try again in a minute.\"}");
                return;
            }

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = "10";
            context.Response.Headers["X-RateLimit-Window"] = "60";
        }

        await _next(context);
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}