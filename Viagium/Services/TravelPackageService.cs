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
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(travelPackage);

            // Valida usando DataAnnotations
            if (!Validator.TryValidateObject(travelPackage, validationContext, validationResults, true))
            {
                var errors = validationResults.Select(vr => vr.ErrorMessage).ToList();
                throw new ArgumentException(string.Join("\n", errors));
            }
            // Validações customizadas
            ValidateCustomRules(travelPackage);

            await _unitOfWork.TravelPackageRepository.AddAsync(travelPackage);
            await _unitOfWork.SaveAsync();

            return travelPackage;
        }

        private void ValidateCustomRules(TravelPackage travelPackage)
        {
            var errors = new List<string>();

            if (travelPackage.OriginAddressId == travelPackage.DestinationAddressId)
                errors.Add("Endereço de origem e destino não podem ser iguais.");

            if (errors.Any())
                throw new ArgumentException(string.Join("\n", errors));
        }
    }
}