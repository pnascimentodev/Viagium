using Viagium.Models;

namespace Viagium.Services;

public interface IUserService
{
    Task<User> AddAync(User user);
}