using System.Threading.Tasks;
using Viagium.EntitiesDTO.Email;

namespace Viagium.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(SendEmailDTO dto);
}
