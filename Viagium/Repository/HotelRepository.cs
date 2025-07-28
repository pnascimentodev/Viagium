using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services;

namespace Viagium.Repository;

public class HotelRepository : IHotelRepository
{   
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ImgbbService _imgbbService;

    public HotelRepository(AppDbContext context, IMapper mapper, ImgbbService imgbbService)
    {
        _context = context;
        _mapper = mapper;
        _imgbbService = imgbbService;
    }
    
    public async Task<HotelWithAddressDTO> AddAsync(HotelCreateFormDTO hotelCreateFormDTO)
    {
        bool exists = await _context.Hotels.Include(h => h.Address)
            .AnyAsync(h => h.Name == hotelCreateFormDTO.Name &&
                h.Address.StreetName == hotelCreateFormDTO.Address.StreetName &&
                h.Address.AddressNumber == hotelCreateFormDTO.Address.AddressNumber &&
                h.Address.Neighborhood == hotelCreateFormDTO.Address.Neighborhood &&
                h.Address.City == hotelCreateFormDTO.Address.City &&
                h.Address.State == hotelCreateFormDTO.Address.State &&
                h.Address.ZipCode == hotelCreateFormDTO.Address.ZipCode &&
                h.Address.Country == hotelCreateFormDTO.Address.Country);
        if (exists)
        {
            throw new InvalidOperationException("Já existe um hotel com este nome neste endereço.");
        }
        string imageUrl = string.Empty;
        if (hotelCreateFormDTO.Image != null)
        {
            imageUrl = await _imgbbService.UploadImageAsync(hotelCreateFormDTO.Image);
        }

        // Mapeia o endereço do DTO para entidade Address
        var address = _mapper.Map<Address>(hotelCreateFormDTO.Address);
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        var addressId = address.AdressId;

        // Mapeia o hotel SEM criar novo Address
        var hotel = _mapper.Map<Hotel>(hotelCreateFormDTO);
        hotel.ImageUrl = imageUrl;
        hotel.AddressId = addressId;
        hotel.Address = null; // Garante que não será criado novo endereço

        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();

        // Atualiza o Address com o HotelId recém-criado
        address.HotelId = hotel.HotelId;
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();

        // Vincula os amenities ao hotel
        if (hotelCreateFormDTO.Amenities != null && hotelCreateFormDTO.Amenities.Any())
        {
            var amenities = await _context.Amenities.Where(a => hotelCreateFormDTO.Amenities.Contains(a.AmenityId)).ToListAsync();
            foreach (var amenity in amenities)
            {
                var hotelAmenity = new HotelAmenity
                {
                    HotelId = hotel.HotelId,
                    AmenityId = amenity.AmenityId
                };
                await _context.HotelAmenities.AddAsync(hotelAmenity);
            }
            await _context.SaveChangesAsync();
        }

        // Buscar o hotel criado com Address e Amenities
        var createdHotel = await _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .FirstOrDefaultAsync(h => h.HotelId == hotel.HotelId);
        var dto = _mapper.Map<HotelWithAddressDTO>(createdHotel);
        dto.Amenities = createdHotel.HotelAmenity
            .Select(ha => new AmenityDTO {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList();
        return dto;
    }
    
    public async Task<HotelWithAddressDTO?> GetByIdAsync(int hotelId)
    {
        var hotel = await _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .FirstOrDefaultAsync(h => h.HotelId == hotelId);

        if (hotel == null) return null;

        var dto = _mapper.Map<HotelWithAddressDTO>(hotel);
        dto.Amenities = hotel.HotelAmenity
            .Select(ha => new AmenityDTO {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList();
        return dto;
    }
    
    public async Task<IEnumerable<HotelWithAddressDTO>> GetAllAsync()
    {
        var hotels = await _context.Hotels
            .Include(h => h.Address)
            .ToListAsync();

        return _mapper.Map<IEnumerable<HotelWithAddressDTO>>(hotels);
    }
    
    public async Task UpdateAsync(HotelWithAddressDTO hotelWithAddressDTO)
    {
        var hotel = await _context.Hotels
            .Include(h => h.Address)
            .FirstOrDefaultAsync(h => h.HotelId == hotelWithAddressDTO.HotelId);

        if (hotel != null)
        {
            _mapper.Map(hotelWithAddressDTO, hotel);
            _context.Entry(hotel).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> DesactivateAsync(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null) return false;

        hotel.IsActive = false;
        _context.Entry(hotel).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ActivateAsync(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null) return false;

        hotel.IsActive = true;
        _context.Entry(hotel).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<IEnumerable<HotelWithAddressDTO>> GetByAmenityAsync(int amenityId)
    {
        var hotels = await _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .Where(h => h.HotelAmenity.Any(ha => ha.AmenityId == amenityId))
            .ToListAsync();
        return hotels.Select(h => {
            var dto = _mapper.Map<HotelWithAddressDTO>(h);
            dto.Amenities = h.HotelAmenity.Select(ha => new AmenityDTO {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList();
            return dto;
        });
    }

    public async Task<IEnumerable<HotelWithAddressDTO>> GetByCityAsync(string city)
    {
        var hotels = await _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .Where(h => h.Address.City.ToLower() == city.ToLower())
            .ToListAsync();
        return hotels.Select(h => {
            var dto = _mapper.Map<HotelWithAddressDTO>(h);
            dto.Amenities = h.HotelAmenity.Select(ha => new AmenityDTO {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList();
            return dto;
        });
    }

    public async Task<IEnumerable<HotelWithAddressDTO>> GetByAmenitiesAsync(List<int> amenityIds)
    {
        var hotels = await _context.Hotels
            .Include(h => h.Address)
            .Include(h => h.HotelAmenity)
                .ThenInclude(ha => ha.Amenity)
            .Where(h => amenityIds.All(id => h.HotelAmenity.Any(ha => ha.AmenityId == id)))
            .ToListAsync();
        return hotels.Select(h => {
            var dto = _mapper.Map<HotelWithAddressDTO>(h);
            dto.Amenities = h.HotelAmenity.Select(ha => new AmenityDTO {
                AmenityId = ha.Amenity.AmenityId,
                Name = ha.Amenity.Name,
                IconName = ha.Amenity.IconName
            }).ToList();
            return dto;
        });
    }
}