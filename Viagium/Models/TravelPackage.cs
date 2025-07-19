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
            [ForeignKey("OriginAddress")]
            public int OriginAddressId { get; set; }
            public Address? OriginAddress { get; set; }

            [Required]
            [ForeignKey("DestinationAddress")]
            public int DestinationAddressId { get; set; }
            public Address? DestinationAddress { get; set; }

            [Required]
            [Url(ErrorMessage = "ImageUrl deve ser uma URL válida")]
            public string ImageUrl { get; set; } = string.Empty;

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "A duração deve ser maior que zero")]
            public int Duration { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "O máximo de pessoas deve ser maior que zero")]
            public int MaxPeople { get; set; }
            
            [Required]
            public string VehicleType { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Currency)]
            [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
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
