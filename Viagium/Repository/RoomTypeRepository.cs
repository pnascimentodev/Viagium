using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository;

public class RoomTypeRepository : IRoomTypeRepository
{
    private readonly AppDbContext _context;
    public RoomTypeRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<RoomType> AddAsync(RoomType roomType)
    {
        await _context.AddAsync(roomType);
        await _context.SaveChangesAsync();
        return roomType;
    }

    public async Task<RoomType?> GetByIdAsync(int id)
    {
        return await _context.RoomTypes
            .Include(rt => rt.RoomTypeAmenities)
            .ThenInclude(rta => rta.Amenity)
            .FirstOrDefaultAsync(rt => rt.RoomTypeId == id && rt.IsActive);
    }

    public async Task<List<RoomType>> GetAllAsync()
    {
        return await _context.RoomTypes
            .Include(rt => rt.RoomTypeAmenities)
            .ThenInclude(rta => rta.Amenity)
            .Where(rt => rt.IsActive)
            .ToListAsync();
    }

    public async Task<List<RoomType>> GetByAmenityId(List<int> amenityIds)
    {
        return await _context.RoomTypes
            .Include(rt => rt.RoomTypeAmenities)
            .ThenInclude(rta => rta.Amenity)
            .Where(rt => amenityIds.All(id => rt.RoomTypeAmenities.Any(rta => rta.Amenity.AmenityId == id)))
            .Where(rt => rt.IsActive)
            .ToListAsync();
    }
    public async Task<List<RoomType>> GetByAmenityIds(List<int> amenityIds)
    {
        return await _context.RoomTypes
            .Include(rt => rt.RoomTypeAmenities)
            .ThenInclude(rta => rta.Amenity)
            .Where(rt => rt.RoomTypeAmenities.Any(rta => amenityIds.Contains(rta.Amenity.AmenityId)))
            .Where(rt => rt.IsActive)
            .ToListAsync();
    }
    public async Task UpdateAsync(RoomType roomType)
    {
        _context.Set<RoomType>().Update(roomType);
        await _context.SaveChangesAsync();
    }
    public async Task<RoomType> DesativateAsync(int id)
    {
        var roomType = await _context.Set<RoomType>().FindAsync(id);
        if (roomType == null) return null;
        roomType.IsActive = false;
        roomType.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return roomType;
    }
    public async Task<RoomType> ActivateAsync(int id)
    {
        var roomType = await _context.Set<RoomType>().FindAsync(id);
        if (roomType == null) return null;
        roomType.IsActive = true;
        roomType.DeletedAt = null;
        await _context.SaveChangesAsync();
        return roomType;
    }
}
