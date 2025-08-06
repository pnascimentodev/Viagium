using Viagium.Models;

namespace Viagium.Repository;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task UpdateAsync(User user);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    Task<bool> DocumentNumberExistsAsync(string documentNumber, int? excludeUserId = null);
    Task<User> DesativateAsync(int id);
    Task<User> ActivateAsync(int id);
    Task<User>GetByEmailAsync(string email, bool includeDeleted = false);
    Task<User>GetEmailByForgotPasswordAsync(string email, bool includeDeleted = false);
    Task<User> UpdatePasswordAsync(int id, string newPassword);
    Task<User> ForgotPasswordAsync(int id, string newPassword);
    Task<List<User>> GetAllActiveAsync();
}