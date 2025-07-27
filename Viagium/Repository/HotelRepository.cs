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
        bool exists = await _context.Hotels.AnyAsync(h => h.Name == hotelCreateFormDTO.Name && h.AddressId == hotelCreateFormDTO.AddressId);
        if (exists)
        {
            throw new InvalidOperationException("Já existe um hotel com este nome neste endereço.");
        }
        string imageUrl = string.Empty;
        if (hotelCreateFormDTO.Image != null)
        {
            imageUrl = await _imgbbService.UploadImageAsync(hotelCreateFormDTO.Image);
        }

        var hotel = _mapper.Map<Hotel>(hotelCreateFormDTO);
        hotel.ImageUrl = imageUrl;

        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();

        // Buscar o hotel criado com Address
        var createdHotel = await _context.Hotels.Include(h => h.Address)
            .FirstOrDefaultAsync(h => h.Name == hotel.Name && h.AddressId == hotel.AddressId);
        return _mapper.Map<HotelWithAddressDTO>(createdHotel);
    }
    
    public async Task<HotelWithAddressDTO?> GetByIdAsync(int hotelId)
    {
        var hotel = await _context.Hotels
            .Include(h => h.Address)
            .FirstOrDefaultAsync(h => h.HotelId == hotelId);

        return hotel != null ? _mapper.Map<HotelWithAddressDTO>(hotel) : null;
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
    
    
    
}