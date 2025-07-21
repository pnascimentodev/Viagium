using System.ComponentModel.DataAnnotations;
using Viagium.Models;
using Viagium.Repository;

namespace Viagium.Services
{
    public class TravelPackageService : ITravelPackage
    {
        private readonly IUnitOfWork _unitOfWork;

        public TravelPackageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TravelPackage> AddAsync(TravelPackage travelPackage)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                // Validação de data annotations
                var validationContext = new ValidationContext(travelPackage);
                Validator.ValidateObject(travelPackage, validationContext, validateAllProperties: true);

                // Validações customizadas específicas do negócio
                ValidateCustomRules(travelPackage);

                await _unitOfWork.TravelPackageRepository.AddAsync(travelPackage);
                await _unitOfWork.SaveAsync();

                return travelPackage;
            }, "criação de pacote de viagem");
        }

        private void ValidateCustomRules(TravelPackage travelPackage)
        {
            var errors = new List<string>();

            if (travelPackage.OriginAddressId == travelPackage.DestinationAddressId)
                errors.Add("Endereço de origem e destino não podem ser iguais.");

            if (errors.Any())
                throw new ArgumentException(string.Join("\n", errors));
        }
        
        public async Task<TravelPackage?> GetByIdAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var travelPackage = await _unitOfWork.TravelPackageRepository.GetByIdAsync(id);
                if (travelPackage == null)
                    throw new KeyNotFoundException("Pacote de viagem não encontrado.");

                return travelPackage;
            }, "busca de pacote de viagem");
        }
    }
}