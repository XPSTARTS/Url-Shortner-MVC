// src/UrlShortner.Domain/Interfaces/IRefreshTokenRepository.cs
using UrlShortner.Domain.Entities;

namespace UrlShortner.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<int> CreateAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<bool> RevokeAsync(string token);
    Task<bool> RevokeAllUserTokensAsync(int userId);
    Task<bool> IsTokenValidAsync(string token);
}