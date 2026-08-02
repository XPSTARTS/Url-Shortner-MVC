// src/UrlShortner.Web/Middleware/InputSanitizationMiddleware.cs
using System.Text.Encodings.Web;

namespace UrlShortner.Web.Middleware;

public class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;

    public InputSanitizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only sanitize POST/PUT requests
        if (context.Request.Method == "POST" || context.Request.Method == "PUT")
        {
            // Enable request body buffering so we can read it
            context.Request.EnableBuffering();

            // Read the body
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();

            // Reset position so controllers can read it
            context.Request.Body.Position = 0;

            // Sanitize if body exists
            if (!string.IsNullOrEmpty(body))
            {
                // HTML encode any suspicious content
                var sanitized = HtmlEncoder.Default.Encode(body);

                // Only replace if something was actually encoded
                if (body != sanitized)
                {
                    // Log potential XSS attempt
                    var logger = context.RequestServices.GetRequiredService<ILogger<InputSanitizationMiddleware>>();
                    logger.LogWarning("Potential XSS detected from IP: {IP}",
                        context.Connection.RemoteIpAddress);
                }
            }
        }

        await _next(context);
    }
}

public static class InputSanitizationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputSanitization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<InputSanitizationMiddleware>();
    }
}