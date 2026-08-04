// src/UrlShortner.Application/Services/EmailService.cs
using System;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace UrlShortner.Application.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService>? _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService>? logger = null)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendOtpEmailAsync(string toEmail, string otp, string purpose)
    {
        var subject = purpose switch
        {
            "Register" => "Verify your email - URL Shortner",
            "Login" => "Your login verification code - URL Shortner",
            _ => "Your verification code - URL Shortner"
        };

        var body = $@"
<div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
    <h2 style='color: #4A90D9;'>URL Shortner</h2>
    <p>Your verification code is:</p>
    <h1 style='background: #f5f5f5; padding: 20px; text-align: center; letter-spacing: 10px; font-size: 36px;'>
        {otp}
    </h1>
    <p>This code will expire in <strong>10 minutes</strong>.</p>
    <p>If you didn't request this code, please ignore this email.</p>
    <hr />
    <small style='color: #999;'>This is an automated message from URL Shortner.</small>
</div>";

        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")
            ?? _configuration["EmailSettings:SmtpHost"];

        if (string.IsNullOrEmpty(smtpHost))
        {
            await SaveOtpToDevFileAsync(toEmail, otp, purpose, subject);
            return;
        }

        await SendRealEmailAsync(toEmail, subject, body);
    }

    private async Task SaveOtpToDevFileAsync(string toEmail, string otp, string purpose, string subject)
    {
        var devFolder = Path.Combine(Directory.GetCurrentDirectory(), "dev-emails");
        Directory.CreateDirectory(devFolder);

        var fileName = $"otp-{purpose.ToLower()}-{toEmail.Replace("@", "-at-")}-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
        var filePath = Path.Combine(devFolder, fileName);

        var content = $@"
============================================
EMAIL (DEV MODE - Not actually sent)
============================================
To: {toEmail}
Subject: {subject}
Purpose: {purpose}
Date: {DateTime.Now}
============================================

Your OTP Code: {otp}

============================================
";

        await File.WriteAllTextAsync(filePath, content);

        Console.WriteLine("");
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║     📧 DEV MODE - OTP Generated          ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine($"║  To:      {toEmail,-30}║");
        Console.WriteLine($"║  Purpose: {purpose,-30}║");
        Console.WriteLine($"║  OTP:     {otp,-30}║");
        Console.WriteLine($"║  File:    {fileName,-30}║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine("");

        _logger?.LogInformation("DEV MODE: OTP for {Email} saved to {FilePath}", toEmail, filePath);
    }

    private async Task SendRealEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")
            ?? _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")
            ?? _configuration["EmailSettings:SmtpPort"] ?? "587");
        var username = Environment.GetEnvironmentVariable("SMTP_USERNAME")
            ?? _configuration["EmailSettings:SmtpUsername"];
        var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
            ?? _configuration["EmailSettings:SmtpPassword"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("SMTP settings missing");

        Console.WriteLine($"📧 Attempting SMTP: {smtpHost}:{smtpPort} with user: {username}");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("URL Shortner", username));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        client.Timeout = 15000;

        // 🔑 Use Auto - it negotiates the best option
        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.Auto);
        Console.WriteLine($"📧 Connected to {smtpHost}");

        await client.AuthenticateAsync(username, password);
        Console.WriteLine($"📧 Authenticated");

        await client.SendAsync(message);
        Console.WriteLine($"📧 Email sent to {toEmail}");

        await client.DisconnectAsync(true);
    }
}