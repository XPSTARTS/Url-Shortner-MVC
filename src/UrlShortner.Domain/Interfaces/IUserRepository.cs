// src/UrlShortner.Domain/Interfaces/IUserRepository.cs
using UrlShortner.Domain.Entities;

namespace UrlShortner.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
}