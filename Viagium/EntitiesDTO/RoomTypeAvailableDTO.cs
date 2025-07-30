using System.Collections.Generic;

namespace Viagium.EntitiesDTO;

public class RoomTypeAvailableDTO
{
    public int RoomTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int NumberOfRoomsAvailable { get; set; }
    public List<string> AvailableRoomNumbers { get; set; } = new();
}
