using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository;

namespace Viagium.Services
{
    public class TravelPackageService : ITravelPackage
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TravelPackageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TravelPackage> AddAsync(CreateTravelPackageDTO travelPackageDto)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                if (travelPackageDto == null)
                    throw new ArgumentNullException(nameof(travelPackageDto), "Os dados do pacote de viagem são obrigatórios");

                // Validação apenas das data annotations
                ExceptionHandler.ValidateObject(travelPackageDto, "pacote de viagem");
                // Validações de negócio específicas
                ValidateCustomRules(travelPackageDto);

                var travelPackage = _mapper.Map<TravelPackage>(travelPackageDto);

                // Auditoria definida apenas na entidade
                travelPackage.CreatedAt = DateTime.Now;
                travelPackage.IsActive = true;

                await _unitOfWork.TravelPackageRepository.AddAsync(travelPackage);
                await _unitOfWork.SaveAsync();

                return travelPackage;
            }, "criação de pacote de viagem");
        }

        private void ValidateCustomRules(CreateTravelPackageDTO travelPackageDto)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(travelPackageDto.OriginAddress) &&
                !string.IsNullOrWhiteSpace(travelPackageDto.DestinationAddress) &&
                string.Equals(travelPackageDto.OriginAddress.Trim(), travelPackageDto.DestinationAddress.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("Endereço de origem e destino não podem ser iguais.");
            }

            // Regras de negócio (não repetir validações básicas)
            if (!string.IsNullOrWhiteSpace(travelPackageDto.ImageUrl))
            {
                if (!Uri.TryCreate(travelPackageDto.ImageUrl, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    errors.Add("A URL da imagem deve ser válida e usar protocolo HTTP ou HTTPS.");
                }
            }

            if (travelPackageDto.HotelId <= 0)
            {
                errors.Add("Um hotel válido deve ser selecionado.");
            }
            if (travelPackageDto.CreatedBy <= 0)
            {
                errors.Add("O usuário criador deve ser válido.");
            }
            if (errors.Any())
            {
                throw new ArgumentException($"Erro de validação:\n{string.Join("\n", errors)}");
            }
        }
    }
}