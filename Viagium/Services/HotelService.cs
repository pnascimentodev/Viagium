using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
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
    
    public HotelService(IUnitOfWork unitOfWork, IMapper mapper, ImgbbService imgbbService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _imgbbService = imgbbService;
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
