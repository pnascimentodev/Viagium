using Viagium.Models;
using System.Threading.Tasks;

namespace Viagium.Repository.Interface
{
    public interface IRoomRepository
    {
        Task<Room?> GetFirstAvailableByRoomTypeIdAsync(int roomTypeId);
        Task UpdateAsync(Room room);
        
    }
}
