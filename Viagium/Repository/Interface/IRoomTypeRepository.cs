using Viagium.Models;

namespace Viagium.Repository.Interface;

public interface IRoomTypeRepository
{
    Task<RoomType> AddAsync(RoomType roomType);
    Task<RoomType?> GetByIdAsync(int id);
    Task<List<RoomType>> GetAllAsync();
    Task<List<RoomType>> GetByAmenityId(List<int> amenityIds);
    Task<List<RoomType>> GetByAmenityIds(List<int> amenityIds);
    Task UpdateAsync(RoomType roomType);
    Task<RoomType> DesativateAsync(int id);
    Task<RoomType> ActivateAsync(int id);
}
