using Microsoft.EntityFrameworkCore;
using Viagium.Data;
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

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Set<User>().ToListAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Set<User>().Update(user);
        await _context.SaveChangesAsync();
    }
    
    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        return await _context.Set<User>()
            .AnyAsync(u => u.Email == email && (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }

    public async Task<bool> DocumentNumberExistsAsync(string documentNumber, int? excludeUserId = null)
    {
        return await _context.Set<User>()
            .AnyAsync(u => u.DocumentNumber == documentNumber && (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }

    public async Task<User> DesativateAsync(int id)
    {
        var existing = await _context.Set<User>().FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Usuário não encontrado para desativação.");

        existing.IsActive = false;
        existing.DeletedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<User> ActivateAsync(int id)
    {
        var existing = await _context.Set<User>().FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Usuário não encontrado para ativação.");

        existing.IsActive = true;
        existing.DeletedAt = null;

        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<User> GetByEmailAsync(string email, bool includeDeleted = false)
    {
        return await _context.Set<User>()
            .Where(u => u.Email == email && (includeDeleted || u.IsActive))
            .FirstOrDefaultAsync();
    }
    
    public async Task<User> UpdatePasswordAsync(int id, string newPassword)
    {
        var user = await _context.Set<User>().FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado para atualização de senha.");

        user.HashPassword = newPassword;
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User> ForgotPasswordAsync(int id, string newPassword)
    {
        var user = await _context.Set<User>().FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("Usuário não encontrado para recuperação de senha.");

        user.HashPassword = newPassword;
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User> GetEmailByForgotPasswordAsync(string email, bool includeDeleted = false)
    {
        return await _context.Set<User>()
            .Where(u => u.Email == email && (includeDeleted || u.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetAllActiveAsync()
    {
        return await _context.Set<User>().Where(u => u.IsActive && u.DeletedAt == null).ToListAsync();
    }

    public async Task<User?> GetByDocumentNumberAsync(string documentNumber)
    {
        return await _context.Set<User>()
            .Where(u => u.DocumentNumber == documentNumber && u.IsActive && u.DeletedAt == null)
            .FirstOrDefaultAsync();
    }
}