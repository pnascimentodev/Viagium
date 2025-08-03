using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Viagium.Models.ENUM;

namespace Viagium.Models;
public class Payment
{
    [Key]
    public int PaymentId { get; set; }
    
    [ForeignKey("Reservation")]
    public int ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    
    [Required]
    public PaymentMethodType PaymentMethod { get; set; }
    
    [StringLength(4)]
    public string? CardLastFourDigits { get; set; }
    
    public string? PaymentIdAsaas { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;
    
    [Required]
    public decimal? Amount { get; set; }
    
    // ✅ CORREÇÃO: PaidAt deve ser null até o pagamento ser confirmado
    public DateTime? PaidAt { get; set; } = null;
}