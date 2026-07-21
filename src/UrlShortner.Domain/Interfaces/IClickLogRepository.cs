// src/UrlShortner.Domain/Interfaces/IClickLogRepository.cs
using UrlShortner.Domain.Entities;

namespace UrlShortner.Domain.Interfaces;

public interface IClickLogRepository
{
    Task CreateAsync(ClickLog clickLog);
    Task<IEnumerable<ClickLog>> GetByShortUrlIdAsync(int shortUrlId, int limit = 100);
    Task<int> GetClickCountByDateRangeAsync(int shortUrlId, DateTime start, DateTime end);
}