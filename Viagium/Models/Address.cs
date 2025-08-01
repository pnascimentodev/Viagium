using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models
{
    public class Address
    {
        [Key]
        public int AdressId { get; set; }
    
        [Display(Name = "Nome da Rua")]
        public string StreetName { get; set; } = string.Empty;
        
        public int AddressNumber { get; set; }
        
        [Display(Name = "Bairro")]
        public string Neighborhood { get; set; } = string.Empty;
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string State { get; set; } = string.Empty;

        [Display(Name = "CEP/Código Postal")]
        public string ZipCode { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O país é obrigatório.")]
        [Display(Name = "País")]
        public string Country { get; set; } = string.Empty;
        
        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [ForeignKey("Affiliate")]
        [Display(Name = "Id do Afiliado")]
        public int? AffiliateId { get; set; }
        public Affiliate? Affiliate { get; set; }
        
        public int? HotelId { get; set; }
        [Display(Name = "Id do Hotel")]
        public Hotel? Hotel { get; set; }
        
        public int? TravelPackageId { get; set; }
        [Display(Name = "Id do Pacote de Viagem")]
        public TravelPackage? TravelPackage { get; set; }
        
        [Display(Name = "Id do Endereço de Origem")]
        public int OriginAddressId { get; set; }
        
        [Display(Name = "Id do Endereço de Destino")]
        public int DestinationAddressId { get; set; }


    }
}
