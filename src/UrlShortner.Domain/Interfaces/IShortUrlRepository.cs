// src/UrlShortner.Domain/Interfaces/IShortUrlRepository.cs
using UrlShortner.Domain.Entities;

namespace UrlShortner.Domain.Interfaces;

public interface IShortUrlRepository
{
    Task<ShortUrl?> GetByCodeAsync(string shortCode);
    Task<IEnumerable<ShortUrl>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(ShortUrl shortUrl);
    Task<bool> UpdateAsync(ShortUrl shortUrl);
    Task<bool> SoftDeleteAsync(int id);
    Task<bool> IncrementClickCountAsync(int id);
    Task<bool> IsCodeUniqueAsync(string shortCode);
}