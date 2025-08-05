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

        // Mapeia apenas os campos do TravelPackage, sem associar objetos Address
        var travelPackage = _mapper.Map<TravelPackage>(createTravelPackageDTO);
        travelPackage.OriginAddressId = originAddressId;
        travelPackage.DestinationAddressId = destinationAddressId;
        travelPackage.OriginAddress = null; // Garante que não será associado objeto Address
        travelPackage.DestinationAddress = null;
        travelPackage.CreatedAt = DateTime.Now; // Garante que o campo CreatedAt será preenchido

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

        // Buscar hotéis ativos na cidade e país de destino (normalizando para ignorar acentuação e espaços)
        var destCity = Normalize(destinationAddressEntity?.City);
        var destCountry = Normalize(destinationAddressEntity?.Country);
        var hotelsQuery = _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .Include(h => h.RoomTypes)
                .ThenInclude(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
            .Include(h => h.RoomTypes)
                .ThenInclude(rt => rt.Rooms)
            .Where(h => h.IsActive);
        var hotels = hotelsQuery
            .AsEnumerable()
            .Where(h => Normalize(h.Address.City) == destCity && Normalize(h.Address.Country) == destCountry)
            .ToList();
        var hotelDtos = hotels.Select(hotel => new HotelWithAddressDTO
        {
            HotelId = hotel.HotelId,
            Name = hotel.Name,
            Description = hotel.Description,
            ContactNumber = hotel.ContactNumber,
            TypeHosting = hotel.TypeHosting,
            IsActive = hotel.IsActive,
            Cnpj = hotel.Cnpj,
            Cadastur = hotel.Cadastur,
            CadasturExpiration = hotel.CadasturExpiration,
            Star = hotel.Star,
            ImageUrl = hotel.ImageUrl,
            CreatedAt = hotel.CreatedAt,
            Address = _mapper.Map<AddressDTO>(hotel.Address),
            Amenities = hotel.HotelAmenity?.Select(ha => new AmenityDTO
            {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList() ?? new List<AmenityDTO>(),
            RoomTypes = _context.RoomTypes
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .Where(rt => rt.HotelId == hotel.HotelId && rt.IsActive)
                .Select(rt => new RoomTypeDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    HotelId = rt.HotelId,
                    Name = rt.Name,
                    Description = rt.Description,
                    ImageUrl = rt.ImageUrl,
                    PricePerNight = rt.PricePerNight,
                    MaxOccupancy = rt.MaxOccupancy,
                    NumberOfRoomsAvailable = rt.NumberOfRoomsAvailable,
                    CreatedAt = rt.CreatedAt,
                    IsActive = rt.IsActive,
                    DeletedAt = rt.DeletedAt,
                    Amenities = rt.RoomTypeAmenities != null ? rt.RoomTypeAmenities.Select(rta => new AmenityDTO
                    {
                        AmenityId = rta.Amenity.AmenityId,
                        Name = rta.Amenity.Name,
                        IconName = rta.Amenity.IconName
                    }).ToList() : new List<AmenityDTO>(),
                    Rooms = rt.Rooms != null ? rt.Rooms.Select(r => _mapper.Map<RoomDTO>(r)).ToList() : new List<RoomDTO>()
                }).ToList(),
            AffiliateId = hotel.AffiliateId
        }).ToList();

        // Cálculo do preço final considerando apenas o desconto manual
        decimal price = travelPackage.OriginalPrice;
        if (travelPackage.ManualDiscountValue > 0)
        {
            price -= (travelPackage.OriginalPrice * travelPackage.ManualDiscountValue / 100);
        }
        travelPackage.Price = price > 0 ? price : 0;

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
            OriginCity = originAddressEntity?.City,
            OriginCountry = originAddressEntity?.Country,
            DestinationCity = destinationAddressEntity?.City,
            DestinationCountry = destinationAddressEntity?.Country,
            Hotels = hotelDtos,
            StartDate = travelPackage.StartDate,
            IsAvailable = travelPackage.IsAvailable
        };
        return response;
    }

    public async Task<bool> UpdateAsync(int travelPackageId, CreateTravelPackageDTO createTravelPackageDTO)
    {
        var travelPackage = await _context.TravelPackages
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);

        if (travelPackage == null)
            return false;

        // Atualiza os campos do pacote de viagem
        travelPackage.Title = createTravelPackageDTO.Title;
        travelPackage.Description = createTravelPackageDTO.Description;
        travelPackage.StartDate = createTravelPackageDTO.StartDate;
        travelPackage.VehicleType = createTravelPackageDTO.VehicleType;
        travelPackage.Duration = createTravelPackageDTO.Duration;
        travelPackage.MaxPeople = createTravelPackageDTO.MaxPeople;
        travelPackage.OriginalPrice = createTravelPackageDTO.OriginalPrice;
        travelPackage.PackageTax = createTravelPackageDTO.PackageTax;
        travelPackage.CupomDiscount = createTravelPackageDTO.CupomDiscount;
        travelPackage.DiscountValue = createTravelPackageDTO.DiscountValue;
        travelPackage.ManualDiscountValue = createTravelPackageDTO.ManualDiscountValue;
        
        // Cálculo do preço final considerando apenas o desconto manual
        decimal price = travelPackage.OriginalPrice;
        if (travelPackage.ManualDiscountValue > 0)
        {
            price -= (travelPackage.OriginalPrice * travelPackage.ManualDiscountValue / 100);
        }
        travelPackage.Price = price > 0 ? price : 0;

        // Atualiza os endereços
        if (createTravelPackageDTO.OriginAddress != null)
        {
            if (travelPackage.OriginAddress == null)
            {
                travelPackage.OriginAddress = new Address();
            }

            travelPackage.OriginAddress.City = createTravelPackageDTO.OriginAddress.City;
            travelPackage.OriginAddress.Country = createTravelPackageDTO.OriginAddress.Country;
        }

        if (createTravelPackageDTO.DestinationAddress != null)
        {
            if (travelPackage.DestinationAddress == null)
            {
                travelPackage.DestinationAddress = new Address();
            }

            travelPackage.DestinationAddress.City = createTravelPackageDTO.DestinationAddress.City;
            travelPackage.DestinationAddress.Country = createTravelPackageDTO.DestinationAddress.Country;
        }

        // Atualiza a imagem se fornecida
        if (createTravelPackageDTO.Image != null)
        {
            var imageUrl = await _imgbbService.UploadImageAsync(createTravelPackageDTO.Image);
            travelPackage.ImageUrl = imageUrl;
        }

        await _context.SaveChangesAsync();
        return true;
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

    public async Task<List<ResponseTravelPackageDTO>> ListAllAsync()
    {
        var now = DateTime.Now;
        var packages = await _context.TravelPackages
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .Include(tp => tp.Reservations)
                .ThenInclude(r => r.Travelers)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.Address)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.HotelAmenity)
                        .ThenInclude(ha => ha.Amenity)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.RoomTypes)
                        .ThenInclude(rt => rt.RoomTypeAmenities)
                            .ThenInclude(rta => rta.Amenity)
            .ToListAsync();

        var result = new List<ResponseTravelPackageDTO>();
        foreach (var pkg in packages)
        {
            var hotels = pkg.PackageHotels?.Select(ph => ph.Hotel).Where(h => h != null).ToList() ?? new List<Models.Hotel>();
            
            // Extrair os IDs dos hotéis já incluídos no pacote
            var existingHotelIds = hotels.Select(h => h.HotelId).ToList();
            
            // Buscar hotéis ativos na cidade/país de destino, e se o pacote tiver CreatedAt, filtrar por data
            var extraHotelsQuery = _context.Hotels
                .Include(h => h.Address)
                .Include(h => h.HotelAmenity)
                    .ThenInclude(ha => ha.Amenity)
                .Include(h => h.RoomTypes)
                    .ThenInclude(rt => rt.RoomTypeAmenities)
                        .ThenInclude(rta => rta.Amenity)
                .Include(h => h.RoomTypes)
                    .ThenInclude(rt => rt.Rooms)
                .Where(h => h.IsActive &&
                            h.Address.City.ToLower() == pkg.DestinationAddress.City.ToLower() &&
                            h.Address.Country.ToLower() == pkg.DestinationAddress.Country.ToLower() &&
                            !existingHotelIds.Contains(h.HotelId));
            if (pkg.CreatedAt != null && pkg.CreatedAt > DateTime.MinValue)
            {
                extraHotelsQuery = extraHotelsQuery.Where(h => h.CreatedAt >= pkg.CreatedAt);
            }
            var extraHotels = await extraHotelsQuery.ToListAsync();
            hotels.AddRange(extraHotels);
            var hotelDtos = hotels.Select(hotel => new HotelWithAddressDTO
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Description = hotel.Description,
                ContactNumber = hotel.ContactNumber,
                TypeHosting = hotel.TypeHosting,
                IsActive = hotel.IsActive,
                Cnpj = hotel.Cnpj,
                InscricaoEstadual = hotel.InscricaoEstadual,
                Cadastur = hotel.Cadastur,
                CadasturExpiration = hotel.CadasturExpiration,
                Star = hotel.Star,
                ImageUrl = hotel.ImageUrl,
                CreatedAt = hotel.CreatedAt,
                Address = _mapper.Map<AddressDTO>(hotel.Address),
                Amenities = hotel.HotelAmenity?.Select(ha => new AmenityDTO
                {
                    AmenityId = ha.Amenity.AmenityId,
                    Name = ha.Amenity.Name,
                    IconName = ha.Amenity.IconName
                }).ToList() ?? new List<AmenityDTO>(),
                RoomTypes = _context.RoomTypes
                    .Include(rt => rt.RoomTypeAmenities)
                        .ThenInclude(rta => rta.Amenity)
                    .Where(rt => rt.HotelId == hotel.HotelId && rt.IsActive)
                    .Select(rt => new RoomTypeDTO
                    {
                        RoomTypeId = rt.RoomTypeId,
                        HotelId = rt.HotelId,
                        Name = rt.Name,
                        Description = rt.Description,
                        ImageUrl = rt.ImageUrl,
                        PricePerNight = rt.PricePerNight,
                        MaxOccupancy = rt.MaxOccupancy,
                        NumberOfRoomsAvailable = rt.NumberOfRoomsAvailable,
                        CreatedAt = rt.CreatedAt,
                        IsActive = rt.IsActive,
                        DeletedAt = rt.DeletedAt,
                        Amenities = rt.RoomTypeAmenities != null ? rt.RoomTypeAmenities.Select(rta => new AmenityDTO
                        {
                            AmenityId = rta.Amenity.AmenityId,
                            Name = rta.Amenity.Name,
                            IconName = rta.Amenity.IconName
                        }).ToList() : new List<AmenityDTO>(),
                }).ToList() ?? new List<RoomTypeDTO>(),
                AffiliateId = hotel.AffiliateId
            }).ToList();
            
            result.Add(new ResponseTravelPackageDTO
            {
                TravelPackageId = pkg.TravelPackageId,
                Title = pkg.Title,
                Description = pkg.Description,
                ImageUrl = pkg.ImageUrl,
                VehicleType = pkg.VehicleType,
                Duration = pkg.Duration,
                MaxPeople = pkg.MaxPeople,
                OriginalPrice = pkg.OriginalPrice,
                Price = pkg.Price,
                PackageTax = pkg.PackageTax,
                CupomDiscount = pkg.CupomDiscount,
                DiscountValue = pkg.DiscountValue,
                StartDate = pkg.StartDate,
                IsAvailable = pkg.IsAvailable,
                OriginCity = pkg.OriginAddress?.City,
                OriginCountry = pkg.OriginAddress?.Country,
                DestinationCity = pkg.DestinationAddress?.City,
                DestinationCountry = pkg.DestinationAddress?.Country,
                IsActive = pkg.IsActive,
                Hotels = hotelDtos,
                CreatedAt = pkg.CreatedAt,
                ConfirmedPeople = pkg.ConfirmedPeople,
            });
        }
        return result;
    }
    public async Task<ResponseTravelPackageDTO?> GetByIdAsync(int id)
    {
        var travelPackage = await _context.TravelPackages
            .Where(tp => tp.IsActive)
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);

        if (travelPackage == null) return null;

        var destCity = Normalize(travelPackage.DestinationAddress?.City);
        var destCountry = Normalize(travelPackage.DestinationAddress?.Country);
        var hotelsQuery = _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .Include(h => h.RoomTypes)
                .ThenInclude(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
            .Include(h => h.RoomTypes)
                .ThenInclude(rt => rt.Rooms)
            .Where(h => h.IsActive);
        var hotels = hotelsQuery
            .AsEnumerable()
            .Where(h => Normalize(h.Address.City) == destCity && Normalize(h.Address.Country) == destCountry)
            .ToList();
        var hotelDtos = hotels.Select(hotel => new HotelWithAddressDTO
        {
            HotelId = hotel.HotelId,
            Name = hotel.Name,
            Description = hotel.Description,
            ContactNumber = hotel.ContactNumber,
            TypeHosting = hotel.TypeHosting,
            IsActive = hotel.IsActive,
            Cnpj = hotel.Cnpj,
            Cadastur = hotel.Cadastur,
            CadasturExpiration = hotel.CadasturExpiration,
            Star = hotel.Star,
            ImageUrl = hotel.ImageUrl,
            Address = _mapper.Map<AddressDTO>(hotel.Address),
            Amenities = hotel.HotelAmenity?.Select(ha => new AmenityDTO
            {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList() ?? new List<AmenityDTO>(),
            RoomTypes = _context.RoomTypes
                .Include(rt => rt.RoomTypeAmenities)
                    .ThenInclude(rta => rta.Amenity)
                .Where(rt => rt.HotelId == hotel.HotelId && rt.IsActive)
                .Select(rt => new RoomTypeDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    HotelId = rt.HotelId,
                    Name = rt.Name,
                    Description = rt.Description,
                    ImageUrl = rt.ImageUrl,
                    PricePerNight = rt.PricePerNight,
                    MaxOccupancy = rt.MaxOccupancy,
                    NumberOfRoomsAvailable = rt.NumberOfRoomsAvailable,
                    CreatedAt = rt.CreatedAt,
                    IsActive = rt.IsActive,
                    DeletedAt = rt.DeletedAt,
                    Amenities = rt.RoomTypeAmenities != null ? rt.RoomTypeAmenities.Select(rta => new AmenityDTO
                    {
                        AmenityId = rta.Amenity.AmenityId,
                        Name = rta.Amenity.Name,
                        IconName = rta.Amenity.IconName
                    }).ToList() : new List<AmenityDTO>(),
                    Rooms = rt.Rooms != null ? rt.Rooms.Select(r => _mapper.Map<RoomDTO>(r)).ToList() : new List<RoomDTO>()
                }).ToList(),
            AffiliateId = hotel.AffiliateId
        }).ToList();

        return new ResponseTravelPackageDTO
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
            StartDate = travelPackage.StartDate
        };
    }
    public async Task<ResponseTravelPackageDTO?> UpdateAsync(ResponseTravelPackageDTO responseTravelPackageDTO)
    {
        // Exemplo de implementação básica (ajuste conforme sua lógica de negócio)
        var travelPackage = await _context.TravelPackages
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == responseTravelPackageDTO.TravelPackageId);
        if (travelPackage == null) return null;

        travelPackage.Title = responseTravelPackageDTO.Title;
        travelPackage.Description = responseTravelPackageDTO.Description;
        travelPackage.ImageUrl = responseTravelPackageDTO.ImageUrl;
        travelPackage.VehicleType = responseTravelPackageDTO.VehicleType;
        travelPackage.Duration = responseTravelPackageDTO.Duration;
        travelPackage.MaxPeople = responseTravelPackageDTO.MaxPeople;
        travelPackage.OriginalPrice = responseTravelPackageDTO.OriginalPrice;
        travelPackage.Price = responseTravelPackageDTO.Price ?? 0;
        travelPackage.PackageTax = responseTravelPackageDTO.PackageTax;
        travelPackage.CupomDiscount = responseTravelPackageDTO.CupomDiscount;
        travelPackage.DiscountValue = responseTravelPackageDTO.DiscountValue;
        // Atualize outros campos conforme necessário

        await _context.SaveChangesAsync();
        return responseTravelPackageDTO;
    }

    public async Task<ResponseTravelPackageDTO?> GetByNameAsync(string name)
    {
        var travelPackage = await _context.TravelPackages
            .Where(tp => tp.IsActive)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.Address)
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .FirstOrDefaultAsync(tp => tp.Title == name);

        if (travelPackage == null) return null;

        return new ResponseTravelPackageDTO
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
            Hotels = _mapper.Map<List<HotelWithAddressDTO>>(travelPackage.PackageHotels.Select(ph => ph.Hotel).Where(h => h != null))
        };
    }

    public async Task<ResponseTravelPackageDTO?> GetByCityAndCountryAsync(string city, string country)
    {
        var travelPackage = await _context.TravelPackages
            .Where(tp => tp.IsActive)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.Address)
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .FirstOrDefaultAsync(tp => tp.DestinationAddress.City == city && tp.DestinationAddress.Country == country);

        if (travelPackage == null) return null;

        return new ResponseTravelPackageDTO
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
            Hotels = _mapper.Map<List<HotelWithAddressDTO>>(travelPackage.PackageHotels.Select(ph => ph.Hotel).Where(h => h != null))
        };
    }

    public async Task<List<ResponseTravelPackageDTO>> DesactivateAsync(int id)
    {
        var travelPackage = await _context.TravelPackages
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);
        if (travelPackage != null)
        {
            travelPackage.IsActive = false;
            travelPackage.DeletedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return await ListAllAsync();
    }

    public async Task<List<ResponseTravelPackageDTO>> ActivateAsync(int id)
    {
        var travelPackage = await _context.TravelPackages
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);
        if (travelPackage != null)
        {
            travelPackage.IsActive = true;
            travelPackage.DeletedAt = null;
            travelPackage.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return await ListAllAsync();
    }

    public async Task<List<ResponseTravelPackageDTO>> CreateDiscountAsync(
        int travelPackageId, decimal discountPercentage, DateTime startDate, DateTime endDate)
    {
        var travelPackage = await _context.TravelPackages
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);
        if (travelPackage != null)
        {
            travelPackage.DiscountValue = discountPercentage;
            await _context.SaveChangesAsync();
        }
        return await ListAllAsync();
    }

    public async Task<List<ResponseTravelPackageDTO>> DesactivateDiscountAsync(int travelPackageId)
    {
        var travelPackage = await _context.TravelPackages
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);
        if (travelPackage != null)
        {
            travelPackage.DiscountValue = 0;
            await _context.SaveChangesAsync();
        }
        return await ListAllAsync();
    }

    public async Task<ResponseTravelPackageDTO?> UpdateCupomAsync(int travelPackageId, string cupom, decimal discountValue)
    {
        var travelPackage = await _context.TravelPackages
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);
        if (travelPackage == null)
            return null;
        travelPackage.CupomDiscount = cupom;
        travelPackage.DiscountValue = discountValue;
        // Recalcula o preço considerando ambos descontos
        decimal price = travelPackage.OriginalPrice;
        if (travelPackage.DiscountValue > 0)
            price -= (travelPackage.OriginalPrice * travelPackage.DiscountValue / 100);
        if (travelPackage.ManualDiscountValue > 0)
            price -= (travelPackage.OriginalPrice * travelPackage.ManualDiscountValue / 100);
        travelPackage.Price = price > 0 ? price : 0;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(travelPackageId);
    }

    public async Task<List<ResponseTravelPackageDTO>> ListAllActiveAsync()
    {
        var packages = await _context.TravelPackages
            .Where(tp => tp.IsActive)
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.Address)
            .ToListAsync();
        var result = new List<ResponseTravelPackageDTO>();
        foreach (var pkg in packages)
        {
            var hotels = pkg.PackageHotels?.Select(ph => ph.Hotel).Where(h => h != null).ToList() ?? new List<Models.Hotel>();
            var hotelDtos = hotels.Select(hotel => _mapper.Map<HotelWithAddressDTO>(hotel)).ToList();
            result.Add(new ResponseTravelPackageDTO
            {
                TravelPackageId = pkg.TravelPackageId,
                Title = pkg.Title,
                Description = pkg.Description,
                ImageUrl = pkg.ImageUrl,
                VehicleType = pkg.VehicleType,
                Duration = pkg.Duration,
                MaxPeople = pkg.MaxPeople,
                OriginalPrice = pkg.OriginalPrice,
                Price = pkg.Price,
                PackageTax = pkg.PackageTax,
                CupomDiscount = pkg.CupomDiscount,
                DiscountValue = pkg.DiscountValue,
                OriginCity = pkg.OriginAddress?.City,
                OriginCountry = pkg.OriginAddress?.Country,
                DestinationCity = pkg.DestinationAddress?.City,
                DestinationCountry = pkg.DestinationAddress?.Country,
                CreatedAt = pkg.CreatedAt,
                IsActive = pkg.IsActive,
                Hotels = hotelDtos,
            });
        }
        return result;
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = value.ToLower().Trim();
        normalized = normalized.Replace("á", "a").Replace("à", "a").Replace("ã", "a").Replace("â", "a")
            .Replace("é", "e").Replace("ê", "e").Replace("è", "e")
            .Replace("í", "i").Replace("î", "i").Replace("ì", "i")
            .Replace("ó", "o").Replace("ô", "o").Replace("õ", "o").Replace("ò", "o")
            .Replace("ú", "u").Replace("û", "u").Replace("ù", "u")
            .Replace("ç", "c");
        normalized = new string(normalized.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c)).ToArray());
        return normalized;
    }
}
