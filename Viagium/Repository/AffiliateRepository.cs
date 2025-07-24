using Viagium.Data;
using Viagium.Models;
using Microsoft.EntityFrameworkCore;
using Viagium.Repository.Interface;

namespace Viagium.Repository;

public class AffiliateRepository : IAffiliateRepository
{
    private readonly AppDbContext _context;

    public AffiliateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Affiliate affiliate)
    {
        await _context.Affiliates.AddAsync(affiliate);
    }

    public async Task UpdateAsync(Affiliate affiliate)
    {
        _context.Affiliates.Update(affiliate);
        await Task.CompletedTask;
    }

    public async Task<Affiliate?> GetByIdAsync(int id)
    {
       return await _context.Affiliates
           .Include(a => a.Address)
           .FirstOrDefaultAsync(a => a.AffiliateId == id);
    }

    public async Task<IEnumerable<Affiliate>> GetAllAsync()
    {
        return await _context.Affiliates
            .Include(a => a.Address)
            .Where(affiliate => affiliate.IsActive)
            .ToListAsync();
    }

    public Task<Affiliate?> GetByCnpjAsync(string cnpj)
    {
        return _context.Affiliates
            .FirstOrDefaultAsync(affiliate => affiliate.Cnpj == cnpj);
    }
    
    public Task<Affiliate?> GetByStateRegistrationAsync(string stateRegistration)
    {
        return _context.Affiliates
            .FirstOrDefaultAsync(affiliate => affiliate.Cnpj == stateRegistration);
    }
}