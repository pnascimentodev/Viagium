using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
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

        public async Task<IEnumerable<TravelPackage>> GetAllAsync()
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var travelPackages= await _unitOfWork.TravelPackageRepository.GetAllAsync();
                if (travelPackages == null || travelPackages.IsNullOrEmpty())
                    throw new KeyNotFoundException("Nenhum pacote de viagem registrado.");

                return travelPackages;
            }, "busca todos os pacotes de viagem");
        }
        
        public async Task<EditTravelPackageDTO> UpdateAsync(int id, EditTravelPackageDTO travelPackage)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var updated = await _unitOfWork.TravelPackageRepository.UpdateAsync(id, travelPackage);
                return updated;
            }, "atualização de pacote de viagem");
        }
        
        public async Task<TravelPackage> DesativateAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var travelPackage = await _unitOfWork.TravelPackageRepository.DesativateAsync(id);
                if (travelPackage == null)
                    throw new KeyNotFoundException("Pacote de viagem não encontrado para desativação.");

                return travelPackage;
            }, "desativação de pacote de viagem");
        }
        
        public async Task<TravelPackage> ActivateAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var travelPackage = await _unitOfWork.TravelPackageRepository.ActivateAsync(id);
                if (travelPackage == null)
                    throw new KeyNotFoundException("Pacote de viagem não encontrado para ativação.");

                return travelPackage;
            }, "ativação de pacote de viagem");
        }
        
        public async Task<TravelPackage> ActivePromotionAsync(int id, decimal discountPercentage)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var travelPackage = await _unitOfWork.TravelPackageRepository.ActivePromotionAsync(id, discountPercentage);
                if (travelPackage == null)
                    throw new KeyNotFoundException("Pacote de viagem não encontrado para ativação de promoção.");
                return travelPackage;
            }, "ativação de promoção de pacote de viagem");
        }
    }
}