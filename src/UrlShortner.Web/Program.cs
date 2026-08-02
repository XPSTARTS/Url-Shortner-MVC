using Serilog;
using UrlShortner.Application.Services;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;
using UrlShortner.Infrastructure.Redis;
using UrlShortner.Infrastructure.Repositories;
using UrlShortner.Web.Extensions;
using UrlShortner.Web.Middleware;

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

    // Load environment variables
    var configBuilder = new ConfigurationBuilder();
    configBuilder.LoadEnvironmentVariables();

    var builder = WebApplication.CreateBuilder(args);

    // 🔑 Add Serilog
    builder.Host.UseSerilog();

    // 🔑 Override settings with environment variables
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

    // 🔑 Override connection strings with env vars
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
    var redisConn = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

    if (!string.IsNullOrEmpty(dbPassword))
    {
        builder.Configuration["ConnectionStrings:DefaultConnection"] =
            $"Server=localhost,1433;Database=UrlShortnerDb;User Id=sa;Password={dbPassword};TrustServerCertificate=True;Encrypt=False;";
    }

    if (!string.IsNullOrEmpty(redisConn))
    {
        builder.Configuration["ConnectionStrings:Redis"] = redisConn;
    }

    if (!string.IsNullOrEmpty(jwtSecret))
    {
        builder.Configuration["JwtSettings:SecretKey"] = jwtSecret;
    }

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
    app.UseSecurityHeaders();
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