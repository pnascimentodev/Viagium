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
           .Include(a => a.Hotels)
               .ThenInclude(h => h.Address)
           .Include(a => a.Hotels)
               .ThenInclude(h => h.HotelAmenity)
                   .ThenInclude(ha => ha.Amenity)
           .FirstOrDefaultAsync(a => a.AffiliateId == id);
    }

    public async Task<IEnumerable<Affiliate>> GetAllAsync()
    {
        return await _context.Affiliates
            .Include(a => a.Address)
            .Include(a => a.Hotels)
                .ThenInclude(h => h.Address)
            .Include(a => a.Hotels)
                .ThenInclude(h => h.HotelAmenity)
                    .ThenInclude(ha => ha.Amenity)
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

    public async Task<IEnumerable<Affiliate>> GetByCityAsync(string city)
    {
        return await _context.Affiliates
            .Include(a => a.Address)
            .Where(a => a.Address.City == city && a.IsActive)
            .ToListAsync();
    }
    
    public async Task<Affiliate?> GetByEmailAsync(string email, bool includeDeleted = false)
    {
        return await _context.Set<Affiliate>()
            .Include(a => a.Address)
            .Include(a => a.Hotels)
            .Where(a => a.Email == email && (includeDeleted || a.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Affiliates
            .AnyAsync(a => a.Email == email && a.IsActive);
    }

    public async Task<bool> CnpjExistsAsync(string cnpj)
    {
        return await _context.Affiliates
            .AnyAsync(a => a.Cnpj == cnpj && a.IsActive);
    }
}
