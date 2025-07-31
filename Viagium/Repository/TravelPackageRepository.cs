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
            OriginCity = travelPackage.OriginAddress?.City,
            OriginCountry = travelPackage.OriginAddress?.Country,
            DestinationCity = travelPackage.DestinationAddress?.City,
            DestinationCountry = travelPackage.DestinationAddress?.Country,
            Hotels = hotelDtos,
            PackageSchedule = _mapper.Map<PackageScheduleDTO>(schedule)
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

        // Atualiza o PackageSchedule
        var schedule = await _context.PackageSchedules
            .FirstOrDefaultAsync(ps => ps.TravelPackageId == travelPackageId);

        if (schedule != null)
        {
            schedule.StartDate = createTravelPackageDTO.PackageSchedule.StartDate;
            schedule.IsFixed = createTravelPackageDTO.PackageSchedule.IsFixed;
            schedule.IsAvailable = createTravelPackageDTO.PackageSchedule.IsAvailable;
        }
        else
        {
            schedule = new PackageSchedule
            {
                TravelPackageId = travelPackageId,
                StartDate = createTravelPackageDTO.PackageSchedule.StartDate,
                IsFixed = createTravelPackageDTO.PackageSchedule.IsFixed,
                IsAvailable = createTravelPackageDTO.PackageSchedule.IsAvailable
            };
            await _context.PackageSchedules.AddAsync(schedule);
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
            .Where(tp => tp.IsActive)
            .Include(tp => tp.PackageSchedules)
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.Address)
            .ToListAsync();

        bool changed = false;
        foreach (var pkg in packages)
        {
            var schedule = pkg.PackageSchedules?.FirstOrDefault();
            if (schedule != null && schedule.IsAvailable && now >= schedule.StartDate)
            {
                schedule.IsAvailable = false;
                changed = true;
            }
        }
        if (changed) await _context.SaveChangesAsync();

        var result = new List<ResponseTravelPackageDTO>();
        foreach (var pkg in packages)
        {
            var hotels = pkg.PackageHotels?.Select(ph => ph.Hotel).Where(h => h != null).ToList() ?? new List<Models.Hotel>();
            var schedule = pkg.PackageSchedules?.FirstOrDefault();
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
                Hotels = _mapper.Map<List<HotelDTO>>(hotels),
                PackageSchedule = _mapper.Map<PackageScheduleDTO>(schedule)
            });
        }
        return result;
    }
    public async Task<ResponseTravelPackageDTO?> GetByIdAsync(int id)
    {
        var travelPackage = await _context.TravelPackages
            .Where(tp => tp.IsActive)
            .Include(tp => tp.PackageHotels)
                .ThenInclude(ph => ph.Hotel)
                    .ThenInclude(h => h.Address)
            .Include(tp => tp.OriginAddress)
            .Include(tp => tp.DestinationAddress)
            .Include(tp => tp.PackageSchedules)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);

        if (travelPackage == null) return null;

        var schedule = travelPackage.PackageSchedules?.FirstOrDefault();
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
            Hotels = _mapper.Map<List<HotelDTO>>(travelPackage.PackageHotels.Select(ph => ph.Hotel).Where(h => h != null)),
            PackageSchedule = _mapper.Map<PackageScheduleDTO>(schedule)
        };
    }
    public async Task<ResponseTravelPackageDTO?> UpdateAsync(ResponseTravelPackageDTO responseTravelPackageDTO)
    {
        // Exemplo de implementação básica (ajuste conforme sua lógica de negócio)
        var travelPackage = await _context.TravelPackages
            .Include(tp => tp.PackageSchedules)
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

        // Atualiza o PackageSchedule (apenas o primeiro, se existir)
        var schedule = travelPackage.PackageSchedules?.FirstOrDefault();
        if (schedule != null && responseTravelPackageDTO.PackageSchedule != null)
        {
            schedule.StartDate = responseTravelPackageDTO.PackageSchedule.StartDate;
            schedule.IsFixed = responseTravelPackageDTO.PackageSchedule.IsFixed;
            schedule.IsAvailable = responseTravelPackageDTO.PackageSchedule.IsAvailable;
        }

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
            .Include(tp => tp.PackageSchedules)
            .FirstOrDefaultAsync(tp => tp.Title == name);

        if (travelPackage == null) return null;

        var schedule = travelPackage.PackageSchedules?.FirstOrDefault();
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
            Hotels = _mapper.Map<List<HotelDTO>>(travelPackage.PackageHotels.Select(ph => ph.Hotel).Where(h => h != null)),
            PackageSchedule = _mapper.Map<PackageScheduleDTO>(schedule)
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
            .Include(tp => tp.PackageSchedules)
            .FirstOrDefaultAsync(tp => tp.DestinationAddress.City == city && tp.DestinationAddress.Country == country);

        if (travelPackage == null) return null;

        var schedule = travelPackage.PackageSchedules?.FirstOrDefault();
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
            Hotels = _mapper.Map<List<HotelDTO>>(travelPackage.PackageHotels.Select(ph => ph.Hotel).Where(h => h != null)),
            PackageSchedule = _mapper.Map<PackageScheduleDTO>(schedule)
        };
    }

    public async Task<List<ResponseTravelPackageDTO>> DesactivateAsync(int id)
    {
        var travelPackage = await _context.TravelPackages
            .Include(tp => tp.PackageSchedules)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);
        if (travelPackage != null)
        {
            travelPackage.IsActive = false;
            travelPackage.DeletedAt = DateTime.Now;
            foreach (var schedule in travelPackage.PackageSchedules)
            {
                schedule.IsAvailable = false;
            }
            await _context.SaveChangesAsync();
        }
        return await ListAllAsync();
    }

    public async Task<List<ResponseTravelPackageDTO>> ActivateAsync(int id)
    {
        var travelPackage = await _context.TravelPackages
            .Include(tp => tp.PackageSchedules)
            .FirstOrDefaultAsync(tp => tp.TravelPackageId == id);
        if (travelPackage != null)
        {
            travelPackage.IsActive = true;
            travelPackage.DeletedAt = null;
            travelPackage.UpdatedAt = DateTime.Now;
            foreach (var schedule in travelPackage.PackageSchedules)
            {
                schedule.IsAvailable = true;
            }
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
}