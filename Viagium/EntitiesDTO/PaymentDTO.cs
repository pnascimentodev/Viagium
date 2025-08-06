using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;
using Viagium.Models.ENUM;

namespace Viagium.EntitiesDTO;

public class PaymentDTO
{
    [Required]
    public PaymentMethodType PaymentMethod { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
    
    [Required]
    public decimal Amount { get; set; }
    
    // ✅ CORREÇÃO: PaidAt deve ser null até o pagamento ser confirmado
    public DateTime? PaidAt { get; set; } = null;
}
