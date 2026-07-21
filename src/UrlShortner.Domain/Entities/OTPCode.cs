// src/UrlShortner.Domain/Entities/OTPCode.cs
namespace UrlShortner.Domain.Entities;

public class OTPCode
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // "Register" or "Login"
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}