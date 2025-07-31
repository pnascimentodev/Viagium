using System.Collections.Generic;

namespace Viagium.EntitiesDTO.TravelPackageDTO
{
    public class ResponseTravelPackageDTO
    {
        public int TravelPackageId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string VehicleType { get; set; }
        public int Duration { get; set; }
        public int MaxPeople { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal? Price { get; set; }
        public decimal PackageTax { get; set; }
        public string CupomDiscount { get; set; }
        public decimal DiscountValue { get; set; }
        public string OriginCity { get; set; }
        public string OriginCountry { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationCountry { get; set; }
        public List<HotelDTO> Hotels { get; set; } = new List<HotelDTO>();
        public PackageScheduleDTO PackageSchedule { get; set; }
    }
}
