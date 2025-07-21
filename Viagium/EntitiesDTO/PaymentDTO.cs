using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class PaymentDTO
{
    
    
    public ReservationDTO? Reservation { get; set; }
    
    [Required]
    public string? PaymentMethod { get; set; }
    
    [StringLength(4)]
    public string? CardLastFourDigits { get; set; }

    public string Status { get; set; } = "Pending";
    
    [Required]
    public decimal Amount { get; set; }
    
    public DateTime? PaidAt { get; set; } = DateTime.Now;
}
