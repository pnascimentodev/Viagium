using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public class TravelPackages
        {
            [Key]
            public int TravelPackagesId { get; set; }

            [Required]
            [StringLength(100)]
            public string Title { get; set; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string Destination { get; set; } = string.Empty;

            [Required]
            public string ImageUrl { get; set; } = string.Empty;

            [Required]
            public int Duration { get; set; }

            [Required]
            public int MaxPeople { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            public decimal Price { get; set; }

            [Required]
            public int CreatedBy { get; set; }

            public DateTime? CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }

            public DateTime? DeletedAt { get; set; }

            public bool IsActive { get; set; } = true;

        }
    }
