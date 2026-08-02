using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;
using UrlShortner.Infrastructure.Redis;
using UrlShortner.Infrastructure.Repositories;
using UrlShortner.Application.Services;
using UrlShortner.Web.Middleware;
using Serilog;  

// ============================================
// SERILOG CONFIGURATION
// ============================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/urlshortner-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting URL Shortner application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllersWithViews();

// Infrastructure
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddScoped<IClickLogRepository, ClickLogRepository>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

// Application
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RefreshTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ShortCodeGenerator>();
builder.Services.AddScoped<UrlValidator>();
builder.Services.AddScoped<UrlShorteningService>();

var app = builder.Build();

// ============================================
// ERROR HANDLING (ORDER MATTERS!)
// ============================================

app.UseGlobalExceptionHandler();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ============================================
// CUSTOM MIDDLEWARE
// ============================================
app.UseRateLimiting();
app.UseJwtCookieAuthentication();
app.UseAuthorization();

// ============================================
// ROUTES
// ============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "redirect",
    pattern: "{shortCode}",
    defaults: new { controller = "Redirect", action = "Index" },
    constraints: new { shortCode = @"^[a-zA-Z0-9\-_]{1,50}$" });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}