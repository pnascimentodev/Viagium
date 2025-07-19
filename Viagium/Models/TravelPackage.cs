using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models
{
    public class TravelPackage
        {
            [Key]
            public int TravelPackagesId { get; set; }
            
            [Required(ErrorMessage = "O Hotel é obrigatório.")]
            [ForeignKey("Hotel")]
            public int HotelId { get; set; }
            public Hotel? Hotel { get; set; }

            [Required(ErrorMessage = "O título é obrigatório.")]
            [StringLength(100)]
            public string Title { get; set; } = string.Empty;

            [Required(ErrorMessage = "A descrição é obrigatória.")]
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;

            [Required(ErrorMessage = "O endereço de origem é obrigatório.")]
            [ForeignKey("OriginAddress")]
            public int OriginAddressId { get; set; }
            public Address? OriginAddress { get; set; }

            [Required(ErrorMessage = "O endereço de destino é obrigatório.")]
            [ForeignKey("DestinationAddress")]
            public int DestinationAddressId { get; set; }
            public Address? DestinationAddress { get; set; }

            [Required(ErrorMessage = "A URL da imagem é obrigatória.")]
            [Url(ErrorMessage = "ImageUrl deve ser uma URL válida")]
            public string ImageUrl { get; set; } = string.Empty;

            [Required(ErrorMessage = "A duração é obrigatória.")]
            [Range(1, int.MaxValue, ErrorMessage = "A duração deve ser maior que zero")]
            public int Duration { get; set; }

            [Required(ErrorMessage = "O máximo de pessoas é obrigatório.")]
            [Range(1, int.MaxValue, ErrorMessage = "O máximo de pessoas deve ser maior que zero")]
            public int MaxPeople { get; set; }
            
            [Required(ErrorMessage = "O tipo de veículo é obrigatório.")]
            public string VehicleType { get; set; } = string.Empty;

            [Required(ErrorMessage = "O preço é obrigatório.")]
            [DataType(DataType.Currency)]
            [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "O usuário criador é obrigatório.")]
            [ForeignKey("User")]
            public int CreatedBy { get; set; }
            public User? User { get; set; }

            public DateTime? CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; }

            public DateTime? DeletedAt { get; set; }

            public bool IsActive { get; set; } = true;
        }
    }
