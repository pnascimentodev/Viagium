using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Viagium.Models.ENUM;

namespace Viagium.Models;
public class Payment
{
    [Key]
    public int PaymentId { get; set; }
    
    [ForeignKey("Reservation")]
    public int? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }
    
    [Required]
    public PaymentMethodType PaymentMethod { get; set; }
    
    [StringLength(4)]
    public string? CardLastFourDigits { get; set; }
    
    public string? PaymentIdAsaas { get; set; }

    public string Status { get; set; } = "Pending";
    
    [Required]
    public decimal? Amount { get; set; }
    
    public DateTime? PaidAt { get; set; } = DateTime.Now;
}