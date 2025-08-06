using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.ApiDTO;

public class AsaasPaymentDTO
{
    public string AsaasApiId { get; set; }
    public decimal TotalPrice { get; set; }
    public string PaymentMethod { get; set; } // "CREDIT_CARD", "BOLETO", etc.
    [DataType(DataType.Date)]
    public DateTime PaymentDate { get; set; }
    public string? CardLastFourDigits { get; set; }
}