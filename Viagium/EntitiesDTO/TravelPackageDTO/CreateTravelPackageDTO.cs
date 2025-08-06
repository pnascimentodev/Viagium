using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO.TravelPackageDTO;

public class CreateTravelPackageDTO
{
            [Required]
            [StringLength(100)]
            public string Title { get; set; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string Description { get; set; } = string.Empty;

            public AddressPackageDTO OriginAddress { get; set; } = new AddressPackageDTO();
            public AddressPackageDTO DestinationAddress { get; set; } = new AddressPackageDTO();

            public IFormFile Image { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Data de Início")]
            public DateTime StartDate { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "A duração deve ser maior que zero")]
            public int Duration { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "O máximo de pessoas deve ser maior que zero")]
            public int MaxPeople { get; set; }
            
            [Display(Name = "Número de pessoas confirmadas")]
            [Range(0, int.MaxValue, ErrorMessage = "O número de pessoas confirmadas deve ser maior ou igual a zero")]
            public int ConfirmedPeople { get; set; } = 0;
            
            [Required]
            public string VehicleType { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Currency)]
            [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
            public decimal OriginalPrice { get; set; }
            
            [DataType(DataType.Currency)]
            public decimal? Price { get; set; } = null;
            
            [Range(0.01, double.MaxValue, ErrorMessage = "Taxa de serviço deve ser maior que zero")]
            public decimal PackageTax { get; set; }
            
            public string CupomDiscount { get; set; } = string.Empty;
            
            [Range(0, 100, ErrorMessage = "O valor do desconto deve ser entre 0 e 100")]
            public decimal DiscountValue { get; set; } = 0;

            [Range(0, 100, ErrorMessage = "O valor do desconto manual deve ser entre 0 e 100")]
            public decimal ManualDiscountValue { get; set; } = 0;


            [Required]
            public int UserId { get; set; }



}