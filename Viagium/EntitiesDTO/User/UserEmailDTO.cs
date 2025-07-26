using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.User;

public class UserEmailDTO
{
    [Required(ErrorMessage = "Não existe um usuário com este email.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public required string Email { get; set; }

}