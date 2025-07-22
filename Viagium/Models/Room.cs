using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [ForeignKey("RoomType")]
    public int RoomTypeId { get; set; }
    public required RoomType RoomType { get; set; }

    [Required]
    public string RoomNumber { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}