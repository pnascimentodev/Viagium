using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;
    public RoomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetFirstAvailableByRoomTypeIdAsync(int roomTypeId)
    {
        return await _context.Rooms
            .Where(r => r.RoomTypeId == roomTypeId && r.IsAvailable)
            .OrderBy(r => r.RoomId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

}
