using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models
{
    public class TravelPackage
        {
            [Key]
            public int TravelPackagesId { get; set; }
            
            [Required]
            [ForeignKey("Hotel")]
            public int HotelId { get; set; }
            public Hotel? Hotel { get; set; }

            [Required]
            [StringLength(100)]
            public string Title { get; set; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string Origin { get; set; } = string.Empty;

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
            public string VehicleType { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Currency)]
            public decimal Price { get; set; }

            [Required]
            [ForeignKey("User")]
            public int CreatedBy { get; set; }
            public User? User { get; set; }

            public DateTime? CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }

            public DateTime? DeletedAt { get; set; }

            public bool IsActive { get; set; } = true;

            
            
      
            
        }
    }
