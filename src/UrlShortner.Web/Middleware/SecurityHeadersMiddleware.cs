// src/UrlShortner.Web/Middleware/SecurityHeadersMiddleware.cs
namespace UrlShortner.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // 🔒 HSTS (only in production)
        if (!context.Request.IsHttps)
        {
            context.Response.Redirect($"https://{context.Request.Host}{context.Request.Path}");
            return;
        }

        // 🔒 Prevent MIME type sniffing
        headers["X-Content-Type-Options"] = "nosniff";

        // 🔒 Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // 🔒 Enable XSS filter
        headers["X-XSS-Protection"] = "1; mode=block";

        // 🔒 Control referrer information
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // 🔒 Content Security Policy
        headers["Content-Security-Policy"] =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
            "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self'";

        // 🔒 Permissions Policy (formerly Feature-Policy)
        headers["Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=(), interest-cohort=()";

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}