// src/UrlShortner.Application/Services/EmailService.cs
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace UrlShortner.Application.Services;

/// <summary>
/// Sends emails using MailKit.
/// Development: Ethereal (fake SMTP)
/// Production: Brevo, SendGrid, etc.
/// </summary>
public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Sends an OTP verification email.
    /// </summary>
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

        await SendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    /// Core email sending logic.
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");

        var fromName = emailSettings["FromName"] ?? "URL Shortner";
        var fromEmail = emailSettings["FromEmail"] ?? "noreply@urlshortner.com";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        var smtpHost = emailSettings["SmtpHost"];
        if (string.IsNullOrEmpty(smtpHost))
        {
            // No SMTP configured - just log for now (development)
            Console.WriteLine($"📧 Email would be sent to {toEmail}: {subject}");
            return;
        }

        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");

        await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

        // For Ethereal/SMTP, authenticate if credentials provided
        var username = emailSettings["SmtpUsername"];
        var password = emailSettings["SmtpPassword"];

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            await client.AuthenticateAsync(username, password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}