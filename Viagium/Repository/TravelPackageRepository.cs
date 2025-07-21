using Viagium.Data;
using Viagium.Models;

namespace Viagium.Repository;

public class TravelPackageRepository : ITravelPackageRepository
{
    private readonly AppDbContext _context;

    public TravelPackageRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(TravelPackage travel)
    {
        await _context.TravelPackages.AddAsync(travel);
    }
    
    public async Task<TravelPackage?> GetByIdAsync(int id)
    {
        return await _context.TravelPackages.FindAsync(id);
    }
}