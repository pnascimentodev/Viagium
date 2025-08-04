using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository;

public class PackageScheduleRepository : IPackageScheduleRepository
{
    private readonly AppDbContext _context;

    public PackageScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PackageSchedule>> GetByTravelPackageIdAsync(int travelPackageId)
    {
        return await _context.PackageSchedules
            .Include(ps => ps.TravelPackage)
            .Where(ps => ps.TravelPackageId == travelPackageId)
            .ToListAsync();
    }

    public async Task<PackageSchedule?> GetByIdAsync(int id)
    {
        return await _context.PackageSchedules
            .Include(ps => ps.TravelPackage)
            .FirstOrDefaultAsync(ps => ps.PackageScheduleId == id);
    }

    public async Task AddAsync(PackageSchedule packageSchedule)
    {
        await _context.PackageSchedules.AddAsync(packageSchedule);
    }

    public async Task UpdateAsync(PackageSchedule packageSchedule)
    {
        _context.PackageSchedules.Update(packageSchedule);
    }

    public async Task DeleteAsync(int id)
    {
        var packageSchedule = await GetByIdAsync(id);
        if (packageSchedule != null)
        {
            _context.PackageSchedules.Remove(packageSchedule);
        }
    }

    public async Task<IEnumerable<PackageSchedule>> GetAllAsync()
    {
        return await _context.PackageSchedules
            .Include(ps => ps.TravelPackage)
            .ToListAsync();
    }
}
