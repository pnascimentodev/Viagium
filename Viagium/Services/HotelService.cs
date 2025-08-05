using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services;

public class HotelService : IHotelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ImgbbService _imgbbService;
    private readonly AppDbContext _context;
    
    public HotelService(IUnitOfWork unitOfWork, IMapper mapper, ImgbbService imgbbService, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _imgbbService = imgbbService;
        _context = context;
    }
    
    public async Task<HotelWithAddressDTO> AddAsync(HotelCreateFormDTO hotelCreateFormDTO)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Upload da imagem se existir
            if (hotelCreateFormDTO.Image != null)
            {
                var imageUrl = await _imgbbService.UploadImageAsync(hotelCreateFormDTO.Image);
                // Atribui a URL ao DTO para ser mapeado corretamente
                hotelCreateFormDTO.GetType().GetProperty("ImageUrl")?.SetValue(hotelCreateFormDTO, imageUrl);
            }
            // Validação de data annotations
            var hotel = _mapper.Map<Hotel>(hotelCreateFormDTO);
            Validator.ValidateObject(hotel, new ValidationContext(hotel), true);
            // Adiciona e retorna o DTO do hotel criado
            var createdHotel = await _unitOfWork.HotelRepository.AddAsync(hotelCreateFormDTO);
            await _unitOfWork.SaveAsync();
            return createdHotel;
        }, "criação de hotel");
    }
    
    public async Task<HotelWithAddressDTO?> GetByIdAsync(int hotelId)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(hotelId);
            if (hotel == null)
                throw new KeyNotFoundException("Hotel não encontrado.");
            return _mapper.Map<HotelWithAddressDTO>(hotel);
        }, "busca de hotel por id");
    }
    
    public async Task<IEnumerable<HotelWithAddressDTO>> GetAllAsync()
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var hotels = await _unitOfWork.HotelRepository.GetAllAsync();
            if (hotels == null || !hotels.Any())
                throw new KeyNotFoundException("Nenhum hotel registrado.");
            return hotels;
        }, "busca todos os hotéis");
    }
    
    public async Task<IEnumerable<HotelWithAddressDTO>> GetAllActiveAsync()
    {
        var hotels = await _unitOfWork.HotelRepository.GetAllActiveAsync();
        return hotels;
    }
    
    public async Task UpdateAsync(HotelWithAddressDTO hotelWithAddressDTO)
    {
        await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var hotel = _mapper.Map<Hotel>(hotelWithAddressDTO);
            Validator.ValidateObject(hotel, new ValidationContext(hotel), true);
            await _unitOfWork.HotelRepository.UpdateAsync(hotelWithAddressDTO);
            await _unitOfWork.SaveAsync();
        }, "atualização de hotel");
    }

    public async Task UpdateAsync(int id, HotelUpdateDTO? hotelUpdateDTO)
    {
        await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            // Buscar o hotel existente completo
            var existingHotelWithAddress = await _unitOfWork.HotelRepository.GetByIdAsync(id);
            if (existingHotelWithAddress == null)
                throw new KeyNotFoundException("Hotel não encontrado.");

            // Buscar o hotel da base de dados para atualização
            var existingHotel = await _context.Hotels
                .Include(h => h.Address)
                .Include(h => h.HotelAmenity)
                .FirstOrDefaultAsync(h => h.HotelId == id);

            if (existingHotel == null)
                throw new KeyNotFoundException("Hotel não encontrado.");

            // Atualizar apenas os campos que foram fornecidos
            existingHotel.Name = !string.IsNullOrEmpty(hotelUpdateDTO.Name) ? hotelUpdateDTO.Name : existingHotel.Name;
            existingHotel.Description = !string.IsNullOrEmpty(hotelUpdateDTO.Description) ? hotelUpdateDTO.Description : existingHotel.Description;
            existingHotel.ContactNumber = !string.IsNullOrEmpty(hotelUpdateDTO.ContactNumber) ? hotelUpdateDTO.ContactNumber : existingHotel.ContactNumber;
            existingHotel.TypeHosting = !string.IsNullOrEmpty(hotelUpdateDTO.TypeHosting) ? hotelUpdateDTO.TypeHosting : existingHotel.TypeHosting;
            existingHotel.Cnpj = !string.IsNullOrEmpty(hotelUpdateDTO.Cnpj) ? hotelUpdateDTO.Cnpj : existingHotel.Cnpj;
            existingHotel.InscricaoEstadual = !string.IsNullOrEmpty(hotelUpdateDTO.InscricaoEstadual) ? hotelUpdateDTO.InscricaoEstadual : existingHotel.InscricaoEstadual;
            existingHotel.Cadastur = !string.IsNullOrEmpty(hotelUpdateDTO.Cadastur) ? hotelUpdateDTO.Cadastur : existingHotel.Cadastur;
            existingHotel.CadasturExpiration = hotelUpdateDTO.CadasturExpiration.HasValue ? hotelUpdateDTO.CadasturExpiration.Value : existingHotel.CadasturExpiration;
            existingHotel.Star = hotelUpdateDTO.Star.HasValue ? hotelUpdateDTO.Star.Value : existingHotel.Star;
            existingHotel.ImageUrl = !string.IsNullOrEmpty(hotelUpdateDTO.ImageUrl) ? hotelUpdateDTO.ImageUrl : existingHotel.ImageUrl;
            existingHotel.IsActive = hotelUpdateDTO.IsActive.HasValue ? hotelUpdateDTO.IsActive.Value : existingHotel.IsActive;

            // Atualizar endereço se fornecido
            if (hotelUpdateDTO.Address != null && existingHotel.Address != null)
            {
                existingHotel.Address.StreetName = !string.IsNullOrEmpty(hotelUpdateDTO.Address.StreetName) ? hotelUpdateDTO.Address.StreetName : existingHotel.Address.StreetName;
                existingHotel.Address.AddressNumber = hotelUpdateDTO.Address.AddressNumber > 0 ? hotelUpdateDTO.Address.AddressNumber : existingHotel.Address.AddressNumber;
                existingHotel.Address.Neighborhood = !string.IsNullOrEmpty(hotelUpdateDTO.Address.Neighborhood) ? hotelUpdateDTO.Address.Neighborhood : existingHotel.Address.Neighborhood;
                existingHotel.Address.City = !string.IsNullOrEmpty(hotelUpdateDTO.Address.City) ? hotelUpdateDTO.Address.City : existingHotel.Address.City;
                existingHotel.Address.State = !string.IsNullOrEmpty(hotelUpdateDTO.Address.State) ? hotelUpdateDTO.Address.State : existingHotel.Address.State;
                existingHotel.Address.ZipCode = !string.IsNullOrEmpty(hotelUpdateDTO.Address.ZipCode) ? hotelUpdateDTO.Address.ZipCode : existingHotel.Address.ZipCode;
                existingHotel.Address.Country = !string.IsNullOrEmpty(hotelUpdateDTO.Address.Country) ? hotelUpdateDTO.Address.Country : existingHotel.Address.Country;
            }

            // Atualizar amenities se fornecido
            if (hotelUpdateDTO.Amenities != null)
            {
                // Remover amenities existentes
                var existingAmenities = existingHotel.HotelAmenity.ToList();
                _context.HotelAmenities.RemoveRange(existingAmenities);

                // Adicionar novos amenities se a lista não estiver vazia
                if (hotelUpdateDTO.Amenities.Any())
                {
                    foreach (var amenityId in hotelUpdateDTO.Amenities)
                    {
                        var hotelAmenity = new HotelAmenity
                        {
                            HotelId = id,
                            AmenityId = amenityId
                        };
                        await _context.HotelAmenities.AddAsync(hotelAmenity);
                    }
                }
            }

            existingHotel.UpdatedAt = DateTime.Now;
            _context.Hotels.Update(existingHotel);
            await _context.SaveChangesAsync();
        }, "atualização parcial de hotel");
    }
    
    public async Task<bool> DesactivateAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var result = await _unitOfWork.HotelRepository.DesactivateAsync(id);
            if (!result)
                throw new KeyNotFoundException("Hotel não encontrado ou já desativado.");
            return result;
        }, "desativação de hotel");
    }
    
    public async Task<bool> ActivateAsync(int id)
    {
        return await ExceptionHandler.ExecuteWithHandling(async () =>
        {
            var result = await _unitOfWork.HotelRepository.ActivateAsync(id);
            if (!result)
                throw new KeyNotFoundException("Hotel não encontrado ou já ativado.");
            return result;
        }, "ativação de hotel");
    }
    
    public async Task<IEnumerable<HotelWithAddressDTO>> GetByAmenityAsync(int amenityId)
    {
        var hotels = await _unitOfWork.HotelRepository.GetByAmenityAsync(amenityId);
        return hotels;
    }

    public async Task<IEnumerable<HotelWithAddressDTO>> GetByCityAsync(string city)
    {
        var hotels = await _unitOfWork.HotelRepository.GetByCityAsync(city);
        return hotels;
    }
    
    public async Task<IEnumerable<HotelWithAddressDTO>> GetByAmenitiesAsync(List<int> amenityIds)
    {
        var hotels = await _unitOfWork.HotelRepository.GetByAmenitiesAsync(amenityIds);
        return hotels;
    }
}
