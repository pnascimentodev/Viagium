using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.TravelPackageDTO
{
    public class ResponseTravelPackageDTO
    {
        public int TravelPackageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int Duration { get; set; }
        public int MaxPeople { get; set; }
        public int ConfirmedPeople { get; set; } = 0;
        public decimal OriginalPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal PackageTax { get; set; }
        public string CupomDiscount { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; } = 0;
        public decimal ManualDiscountValue { get; set; }
        
        [DataType(DataType.Date)]
        [Display(Name = "Data de Início")]
        public DateTime StartDate { get; set; }
        
        [Display(Name = "Está disponível?")]
        public bool IsAvailable { get; set; } = true;
        
        public string OriginCity { get; set; } = string.Empty;
        public string OriginCountry { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationCountry { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public List<HotelWithAddressDTO> Hotels { get; set; } = new List<HotelWithAddressDTO>();
    }
}
