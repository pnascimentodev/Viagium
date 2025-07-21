using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Models;

namespace Viagium.Repository;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(User user)
    {
        await _context.AddAsync(user);
    }
    
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Set<User>().FindAsync(id);
    }
}