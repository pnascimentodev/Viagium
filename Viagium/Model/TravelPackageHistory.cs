using System.ComponentModel.DataAnnotations;

namespace Viagium.Model
{
    public class TravelPackageHistory
    {
        [Key]
        public int TravelerPackageHistoryId { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Título")]
        public string? Title { get; set; }

        [Required]
        [MaxLength(2000)]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Destino")]
        public string? Destination { get; set; }

        [Required]
        [Display(Name = "Duração")]
        public int Duration { get; set; }

        [Required]
        [Display(Name = "Preço")]
        public decimal Price { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now; 

    }
}
