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
        return _mapper.Map<IEnumerable<AmenityDTO>>(amenities);
    }
}