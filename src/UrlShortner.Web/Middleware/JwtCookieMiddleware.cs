// src/UrlShortner.Web/Middleware/JwtCookieMiddleware.cs
using UrlShortner.Application.Services;
using System.Security.Claims;

namespace UrlShortner.Web.Middleware;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;

    public JwtCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, JwtTokenService jwtTokenService)
    {
        var token = context.Request.Cookies["AccessToken"];

        if (!string.IsNullOrEmpty(token))
        {
            var principal = jwtTokenService.ValidateToken(token);

            if (principal != null)
            {
                // Token is valid - set the user
                context.User = principal;
            }
            else
            {
                // Token expired or invalid - clear the cookie
                context.Response.Cookies.Delete("AccessToken");
            }
        }

        await _next(context);
    }
}

// Extension method to easily add middleware
public static class JwtCookieMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtCookieAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtCookieMiddleware>();
    }
}