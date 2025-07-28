using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository;

public class AmenityRepository : IAmenityRepository
{
    private readonly AppDbContext _context;
    public AmenityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Amenity?> GetByIdAsync(int id)
    {
        return await _context.Amenities.FindAsync(id);
    }

    public async Task<List<Amenity>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Amenities.Where(a => ids.Contains(a.AmenityId)).ToListAsync();
    }

    public async Task<Amenity?> GetByNameAsync(string name)
    {
        return await _context.Amenities.FirstOrDefaultAsync(a => a.Name == name);
    }

    public async Task<List<Amenity>> GetAllAsync()
    {
        return await _context.Amenities.ToListAsync();
    }
}
