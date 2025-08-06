using Viagium.EntitiesDTO;

namespace Viagium.Services.Interfaces;

public interface IRoomTypeService
{
    Task<RoomTypeDTO> AddAsync(RoomTypeCreateDTO dto);
    Task<RoomTypeDTO?> GetByIdAsync(int id);
    Task<List<RoomTypeDTO>> GetAllAsync();
    Task<List<RoomTypeDTO>> GetAllActiveAsync();
    Task<List<RoomTypeDTO>> GetByAmenityId(List<int> amenityIds);
    Task<List<RoomTypeDTO>> GetByAmenityIds(List<int> amenityIds);
    Task<RoomTypeDTO> UpdateAsync(RoomTypeUpdateDTO dto);
    Task<RoomTypeDTO> DesativateAsync(int id);
    Task<RoomTypeDTO> ActivateAsync(int id);
    Task<List<RoomTypeDTO>> GetRoomTypesWithAvailableRoomsAsync();
    Task<List<RoomTypeDTO>> GetRoomTypesWithUnavailableRoomsAsync();
}
