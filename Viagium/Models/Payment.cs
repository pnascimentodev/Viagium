using System.ComponentModel.DataAnnotations;

namespace Viagium.Models;

public class Payment
{
    [Key]
    public int PaymentId { get; set; }
    
    public int ReservationId { get; set; }
    
    [Required]
    public string? PaymentMethod { get; set; }
    
    [Required]
    [StringLength(4)]
    public string? CardLastFourDigits { get; set; }

    public string Status { get; set; } = "Pending";
    
    [Required]
    public decimal Amount { get; set; }
    
    public DateTime PaidAt { get; set; } = DateTime.Now;
}