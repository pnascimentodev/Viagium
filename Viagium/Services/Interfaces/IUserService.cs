using Viagium.Models;

namespace Viagium.Services;

public interface IUserService
{
    Task<User> AddAync(User user, string password);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task UpdateAsync(User user, string password);
    Task<User> DesativateAsync(int id);
    Task<User> ActivateAsync(int id);
}