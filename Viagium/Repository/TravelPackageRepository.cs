using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.TravelPackageDTO;
using Viagium.Models;
using Viagium.Services;

namespace Viagium.Repository;

public class TravelPackageRepository : ITravelPackageRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ImgbbService _imgbbService;

    public TravelPackageRepository(AppDbContext context, IMapper mapper, ImgbbService imgbbService)
    {
        _context = context;
        _mapper = mapper;
        _imgbbService = imgbbService;
    }
    
    public async Task<ResponseTravelPackageDTO> AddAsync(CreateTravelPackageDTO createTravelPackageDTO)
    {
        // Se o OriginAddressId/DestinationAddressId vier preenchido, use, senão crie novo Address
        int originAddressId;
        if (createTravelPackageDTO.OriginAddress != null && createTravelPackageDTO.OriginAddress.AddressId > 0)
        {
            originAddressId = createTravelPackageDTO.OriginAddress.AddressId;
        }
        else
        {
            var originAddress = new Address
            {
                City = createTravelPackageDTO.OriginAddress.City,
                Country = createTravelPackageDTO.OriginAddress.Country,
                CreatedAt = createTravelPackageDTO.OriginAddress.CreatedAt ?? DateTime.Now
            };
            await _context.Addresses.AddAsync(originAddress);
            await _context.SaveChangesAsync();
            originAddressId = originAddress.AdressId;
        }

        int destinationAddressId;
        if (createTravelPackageDTO.DestinationAddress != null && createTravelPackageDTO.DestinationAddress.AddressId > 0)
        {
            destinationAddressId = createTravelPackageDTO.DestinationAddress.AddressId;
        }
        else
        {
            var destinationAddress = new Address
            {
                City = createTravelPackageDTO.DestinationAddress.City,
                Country = createTravelPackageDTO.DestinationAddress.Country,
                CreatedAt = createTravelPackageDTO.DestinationAddress.CreatedAt ?? DateTime.Now
            };
            await _context.Addresses.AddAsync(destinationAddress);
            await _context.SaveChangesAsync();
            destinationAddressId = destinationAddress.AdressId;
        }

        var travelPackage = _mapper.Map<TravelPackage>(createTravelPackageDTO);
        travelPackage.OriginAddressId = originAddressId;
        travelPackage.DestinationAddressId = destinationAddressId;
        if (createTravelPackageDTO.Image != null)
        {
            var imageUrl = await _imgbbService.UploadImageAsync(createTravelPackageDTO.Image);
            travelPackage.ImageUrl = imageUrl;
        }
        await _context.TravelPackages.AddAsync(travelPackage);
        await _context.SaveChangesAsync();

        // Atualiza os endereços com o TravelPackageId
        var originAddressEntity = await _context.Addresses.FirstOrDefaultAsync(a => a.AdressId == originAddressId);
        if (originAddressEntity != null)
        {
            originAddressEntity.TravelPackageId = travelPackage.TravelPackageId;
        }
        var destinationAddressEntity = await _context.Addresses.FirstOrDefaultAsync(a => a.AdressId == destinationAddressId);
        if (destinationAddressEntity != null)
        {
            destinationAddressEntity.TravelPackageId = travelPackage.TravelPackageId;
        }
        await _context.SaveChangesAsync();

        // Salvar o PackageSchedule associado ao pacote
        var schedule = new PackageSchedule
        {
            TravelPackageId = travelPackage.TravelPackageId,
            StartDate = createTravelPackageDTO.PackageSchedule.StartDate,
            IsFixed = createTravelPackageDTO.PackageSchedule.IsFixed,
            IsAvailable = createTravelPackageDTO.PackageSchedule.IsAvailable
        };
        await _context.PackageSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();

        // Buscar hotéis ativos na cidade e país de destino
        var hotels = await _context.Hotels
            .Include(h => h.Address)
            .Where(h => h.IsActive &&
                        h.Address.City.ToLower() == travelPackage.DestinationAddress.City.ToLower() &&
                        h.Address.Country.ToLower() == travelPackage.DestinationAddress.Country.ToLower())
            .ToListAsync();
        var hotelDtos = _mapper.Map<List<HotelDTO>>(hotels);

        // Montar o DTO de resposta
        var response = new ResponseTravelPackageDTO
        {
            TravelPackageId = travelPackage.TravelPackageId,
            Title = travelPackage.Title,
            Description = travelPackage.Description,
            ImageUrl = travelPackage.ImageUrl,
            VehicleType = travelPackage.VehicleType,
            Duration = travelPackage.Duration,
            MaxPeople = travelPackage.MaxPeople,
            OriginalPrice = travelPackage.OriginalPrice,
            Price = travelPackage.Price,
            PackageTax = travelPackage.PackageTax,
            CupomDiscount = travelPackage.CupomDiscount,
            DiscountValue = travelPackage.DiscountValue,
            OriginCity = travelPackage.OriginAddress?.City,
            OriginCountry = travelPackage.OriginAddress?.Country,
            DestinationCity = travelPackage.DestinationAddress?.City,
            DestinationCountry = travelPackage.DestinationAddress?.Country,
            Hotels = hotelDtos,
            PackageSchedule = _mapper.Map<PackageScheduleDTO>(schedule)
        };
        return response;
    }
    
    public async Task<CreateTravelPackageDTO?> AssociateActiveHotelsByCityAndCountry(
        int travelPackageId, string city, string country)
    {
        var travelPackage = await _context.TravelPackages
            .Include(tp => tp.PackageHotels)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);

        if (travelPackage == null)
            return null;

        var hotels = await _context.Hotels
            .Where(h => h.IsActive && h.Address.City.ToLower() == city.ToLower() && h.Address.Country.ToLower() == country.ToLower())
            .ToListAsync();

        foreach (var hotel in hotels)
        {
            travelPackage.PackageHotels.Add(new PackageHotel
            {
                TravelPackageId = travelPackage.TravelPackageId,
                HotelId = hotel.HotelId
            });
        }

        await _context.SaveChangesAsync();
        return _mapper.Map<CreateTravelPackageDTO>(travelPackage);
    }
}