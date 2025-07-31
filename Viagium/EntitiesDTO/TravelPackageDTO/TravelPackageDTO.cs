using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO.TravelPackageDTO;

namespace Viagium.EntitiesDTO.TravelPackageDTO;

public class TravelPackageDTO
{
    public int TravelPackageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public AddressPackageDTO? Address { get; set; }
    public int Duration { get; set; }
    public int MaxPeople { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal Price { get; set; }
    public decimal PackageTax { get; set; }
    public string CupomDiscount { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public PackageScheduleDTO? PackageSchedule { get; set; }
    // Adicione outros campos necessários conforme sua model/entity
}

