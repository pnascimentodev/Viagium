using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;

namespace Viagium.Services;

public class AmenityService : IAmenityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public AmenityService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<AmenityDTO>> GetAllAsync()
    {
        var amenities = await _unitOfWork.AmenityRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AmenityDTO>>(amenities.OrderBy(a => a.Name));
    }

    public async Task<IEnumerable<AmenityDTO>> GetHotelAmenitiesAsync()
    {
        var amenities = await _unitOfWork.AmenityRepository.GetAllAsync();
        var hotelAmenities = amenities.Where(a => a.AmenityScope == "Hotel" || a.AmenityScope == "hotel" ||a.AmenityScope == "Ambos");
        return _mapper.Map<IEnumerable<AmenityDTO>>(hotelAmenities.OrderBy(a => a.Name));
    }

    public async Task<IEnumerable<AmenityDTO>> GetRoomTypeAmenitiesAsync()
    {
        var amenities = await _unitOfWork.AmenityRepository.GetAllAsync();
        var roomTypeAmenities = amenities.Where(a => a.AmenityScope == "Quarto" || a.AmenityScope == "quarto" ||a.AmenityScope == "Ambos");
        return _mapper.Map<IEnumerable<AmenityDTO>>(roomTypeAmenities.OrderBy(a => a.Name));
    }
}