using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IAmenityRepository _amenityRepository;
    private readonly IMapper _mapper;

    public RoomTypeService(IRoomTypeRepository roomTypeRepository, IAmenityRepository amenityRepository, IMapper mapper)
    {
        _roomTypeRepository = roomTypeRepository;
        _amenityRepository = amenityRepository;
        _mapper = mapper;
    }

    public async Task<RoomTypeDTO> AddAsync(RoomTypeCreateDTO dto)
    {
        var roomType = _mapper.Map<RoomType>(dto);
        roomType.IsActive = true;
        Validator.ValidateObject(roomType, new ValidationContext(roomType), true);

        var amenities = await _amenityRepository.GetByIdsAsync(dto.Amenities);

        roomType.RoomTypeAmenities = amenities
            .Select(a => new RoomTypeAmenity { AmenityId = a.AmenityId, Amenity = a, RoomType = roomType })
            .ToList();

        var created = await _roomTypeRepository.AddAsync(roomType);
        // For�a o carregamento dos amenities para o retorno
        return _mapper.Map<RoomTypeDTO>(created);
    }

    public async Task<RoomTypeDTO?> GetByIdAsync(int id)
    {
        var roomType = await _roomTypeRepository.GetByIdAsync(id);
        if (roomType == null) return null;
        return _mapper.Map<RoomTypeDTO>(roomType);
    }

    public async Task<List<RoomTypeDTO>> GetAllAsync()
    {
        var roomTypes = await _roomTypeRepository.GetAllAsync();
        return roomTypes.Select(rt => _mapper.Map<RoomTypeDTO>(rt)).ToList();
    }

    public async Task<List<RoomTypeDTO>> GetByAmenityId(List<int> amenityIds)
    {
        var roomTypes = await _roomTypeRepository.GetByAmenityId(amenityIds);
        return roomTypes.Select(rt => _mapper.Map<RoomTypeDTO>(rt)).ToList();
    }

    public async Task<List<RoomTypeDTO>> GetByAmenityIds(List<int> amenityIds)
    {
        var roomTypes = await _roomTypeRepository.GetByAmenityIds(amenityIds);
        return roomTypes.Select(rt => _mapper.Map<RoomTypeDTO>(rt)).ToList();
    }

    public async Task<RoomTypeDTO> UpdateAsync(RoomTypeUpdateDTO dto)
    {
        var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
        if (roomType == null)
            throw new KeyNotFoundException("Tipo de quarto n�o encontrado para atualiza��o.");

        _mapper.Map(dto, roomType);
        Validator.ValidateObject(roomType, new ValidationContext(roomType), true);

        await _roomTypeRepository.UpdateAsync(roomType);
        return _mapper.Map<RoomTypeDTO>(roomType);
    }

    public async Task<RoomTypeDTO> DesativateAsync(int id)
    {
        var roomType = await _roomTypeRepository.DesativateAsync(id);
        if (roomType == null)
            throw new KeyNotFoundException("Tipo de quarto n�o encontrado para desativa��o.");

        return _mapper.Map<RoomTypeDTO>(roomType);
    }

    public async Task<RoomTypeDTO> ActivateAsync(int id)
    {
        var roomType = await _roomTypeRepository.ActivateAsync(id);
        if (roomType == null)
            throw new KeyNotFoundException("Tipo de quarto n�o encontrado para ativa��o.");
        return _mapper.Map<RoomTypeDTO>(roomType);
    }
}
