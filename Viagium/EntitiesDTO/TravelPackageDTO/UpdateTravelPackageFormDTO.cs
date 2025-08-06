using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Viagium.EntitiesDTO.TravelPackageDTO
{
    public class UpdateTravelPackageFormDTO
    {
        public int TravelPackageId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string VehicleType { get; set; }
        public int Duration { get; set; }
        public int MaxPeople { get; set; }
        public int ConfirmedPeople { get; set; } = 0;
        public decimal OriginalPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal PackageTax { get; set; }
        public string CupomDiscount { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; } = 0;
        public decimal ManualDiscountValue { get; set; } = 0;
        public string OriginCity { get; set; }
        public string OriginCountry { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationCountry { get; set; }
        public IFormFile ImageFile { get; set; }
        // Se necessário, adicione outras propriedades relacionadas a hotéis e agendamento
    }
}
