using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models
{
    public class TravelPackage
        {
            [Key]
            public int TravelPackageId { get; set; }

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
            [DataType(DataType.Date)]
            [Display(Name = "Data de Início")]
            public DateTime StartDate { get; set; }

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "A duração deve ser maior que zero")]
            public int Duration { get; set; }

            [NotMapped]
            [Display(Name = "Data de Término")]
            public DateTime EndDate => StartDate.AddDays(Duration - 1);

            [Required]
            [Range(1, int.MaxValue, ErrorMessage = "O máximo de pessoas deve ser maior que zero")]
            public int MaxPeople { get; set; }
            
            [Display(Name = "Número de pessoas confirmadas")]
            [Range(0, int.MaxValue, ErrorMessage = "O número de pessoas confirmadas deve ser maior ou igual a zero")]
            public int ConfirmedPeople { get; set; } = 0;
            
            [Display(Name = "Está ativo?")]
            public bool IsActive { get; set; } = true;
            
            [Display(Name = "Está disponível?")]
            public bool IsAvailable { get; set; } = true;
            
            [Required]
            public string VehicleType { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Currency)]
            [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
            public decimal OriginalPrice { get; set; }
            
            [DataType(DataType.Currency)]
            [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
            public decimal Price { get; set; } = 0;
            
            [Range(0.01, double.MaxValue, ErrorMessage = "Taxa de serviço deve ser maior que zero")]
            public decimal PackageTax { get; set; }
            
            public string CupomDiscount { get; set; } = string.Empty;
            
            [Range(0, 100, ErrorMessage = "O valor do desconto deve ser entre 0 e 100")]
            public decimal DiscountValue { get; set; } = 0;

            [Range(0, 100, ErrorMessage = "O valor do desconto manual deve ser entre 0 e 100")]
            public decimal ManualDiscountValue { get; set; } = 0;

            [Required]
            [ForeignKey("User")]
            public int CreatedBy { get; set; }
            public User? User { get; set; }

           

            public DateTime? CreatedAt { get; set; } = DateTime.Now;
            public DateTime? UpdatedAt { get; set; } 
            public DateTime? DeletedAt { get; set; }
            
            public virtual ICollection<Reservation>? Reservations { get; set; }
            public virtual ICollection<PackageHotel> PackageHotels { get; set; } = new List<PackageHotel>();
        
    }
}
