using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository;
using Viagium.Services.Interfaces;

namespace Viagium.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public AddressService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AddressDTO> AddAsync(AddressDTO addressDto)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var address = _mapper.Map<Address>(addressDto);
                // Validação de data annotations
                Validator.ValidateObject(address, new ValidationContext(address), true);
                // Validações customizadas específicas do negócio
                ValidateCustomRules(address);
                await _unitOfWork.AddressRepository.AddAsync(address);
                await _unitOfWork.SaveAsync();
                return _mapper.Map<AddressDTO>(address);
            }, "criação de endereço");
        }

        public async Task<IEnumerable<AddressDTO>> GetAllAsync()
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var addresses = await _unitOfWork.AddressRepository.GetAllAsync();
                if (addresses == null || !addresses.Any())
                    throw new KeyNotFoundException("Nenhum endereço registrado.");

                return _mapper.Map<IEnumerable<AddressDTO>>(addresses);
            }, "busca todos os endereços");
        }


        public async Task<AddressDTO?> GetByIdAsync(int id)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var address = await _unitOfWork.AddressRepository.GetByIdAsync(id);
                if (address == null)
                    throw new KeyNotFoundException("Endereço por id não encontrado.");

                return _mapper.Map<AddressDTO>(address);
            }, "busca de endereço de viagem");
        }


        public async Task<AddressDTO> UpdateAsync(int id, AddressDTO addressDto)
        {
            return await ExceptionHandler.ExecuteWithHandling(async () =>
            {
                var address = _mapper.Map<Address>(addressDto);
                // Validação de data annotations
                Validator.ValidateObject(address, new ValidationContext(address), true);

                // Validações customizadas específicas do negócio
                ValidateCustomRules(address);

                // Verifica se o endereço existe
                var existingAddress = await _unitOfWork.AddressRepository.GetByIdAsync(id);
                if (existingAddress == null)
                    throw new KeyNotFoundException("Endereço para atualização não encontrado.");

                // Atualiza os campos
                existingAddress.StreetName = address.StreetName;
                existingAddress.AddressNumber = address.AddressNumber;
                existingAddress.Neighborhood = address.Neighborhood;
                existingAddress.City = address.City;
                existingAddress.State = address.State;
                existingAddress.ZipCode = address.ZipCode;
                existingAddress.Country = address.Country;
                existingAddress.AffiliateId = address.AffiliateId;
                existingAddress.HotelId = address.HotelId;

                await _unitOfWork.AddressRepository.UpdateAsync(existingAddress);
                await _unitOfWork.SaveAsync();

                return _mapper.Map<AddressDTO>(existingAddress);
            }, "atualização de endereço");
        }



        private void ValidateCustomRules(Address address)
        {
            var errors = new List<string>();

            if (address.AddressNumber <= 0)
                errors.Add("Numero do endereço não pode ser negativo.");

            if (errors.Any())
                throw new ArgumentException(string.Join("\n", errors));
        }

    }
}
