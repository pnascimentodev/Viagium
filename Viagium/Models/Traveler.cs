namespace Viagium.Models;

public class Traveler
{
    [key]
    public int TravelersId { get; set; }
    public int reservationId { get; set; } 
    [Requerid]
    public string FistName { get; set; }
    [Requerid]
    public string LastName { get; set; }
    [Requerid]
    public string DocumentNumber { get; set; }
    [Requerid]
    public DateTime DateOfBirth { get; set; } //Verificar se vai ser DateTime ou DateOnly
}