using UrlShortner.Domain.Interfaces;
using UrlShortner.Infrastructure.Data;
using UrlShortner.Infrastructure.Redis;
using UrlShortner.Infrastructure.Repositories;
using UrlShortner.Application.Services;
using UrlShortner.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

// Error handling
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Middleware
app.UseRateLimiting();
app.UseJwtCookieAuthentication();
app.UseAuthorization();

// Routes - SPECIFIC FIRST, CATCH-ALL LAST
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "redirect",
    pattern: "{shortCode}",
    defaults: new { controller = "Redirect", action = "Index" },
    constraints: new { shortCode = @"^[a-zA-Z0-9\-_]{1,50}$" });

app.Run();