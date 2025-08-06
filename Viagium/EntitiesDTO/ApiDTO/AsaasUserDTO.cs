namespace Viagium.EntitiesDTO.ApiDTO;

public class AsaasUserDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; } // Email do cliente
    public string? Phone { get; set; } // Telefone do cliente
    public string? DocumentNumber { get; set; } // CPF ou CNPJ do cliente
}