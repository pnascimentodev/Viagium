using Viagium.Models;

namespace Viagium.Services;

public interface IUserService
{
    Task<User> AddAync(User user);
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task UpdateAsync(User user);
}