using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int Rating { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
