namespace Viagium.EntitiesDTO;

public class AffiliateDTO
{
    public string Name { get; set; }
    public string Cnpj { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public string StateRegistration { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<HotelDTO>? Hotels { get; set; }
}
