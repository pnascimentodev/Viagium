using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public class TravelPackages
    {
        [Key]
        public int TravelPackagesId {get; set;}
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Destino")]
        [StringLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Duração")]
        public string Duration { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Preço")]

        public double Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        public Boolean IsActived { get; set; } = true;

        public DateTime? DeletedAt { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Criado por")]
        public int CreatedByEmployeeId { get; set; }





    }
}
