using System.ComponentModel.DataAnnotations;
using Viagium.Models.ENUM;

namespace Viagium.EntitiesDTO.ApiDTO;

public class AsaasPaymentDTO
{
    public string AsaasApiId { get; set; }
    public decimal TotalPrice { get; set; }
    public PaymentMethodType PaymentMethod { get; set; } // "CREDIT_CARD", "BOLETO", etc.
    public string? CardLastFourDigits { get; set; }
    public int ReservationId { get; set; }
}