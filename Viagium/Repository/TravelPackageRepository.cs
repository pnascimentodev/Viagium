using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.EntitiesDTO;
using Viagium.Models;

namespace Viagium.Repository;

public class TravelPackageRepository : ITravelPackageRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public TravelPackageRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task AddAsync(TravelPackage travel)
    {
        await _context.TravelPackages.AddAsync(travel);
    }

    public async Task<IEnumerable<TravelPackage>> GetAllAsync()
    {
        return await _context.TravelPackages.ToListAsync();
    }

    public async Task<TravelPackage?> GetByIdAsync(int id)
    {
        return await _context.TravelPackages.FindAsync(id);
    }
    
    public async Task<EditTravelPackageDTO> UpdateAsync(int id, EditTravelPackageDTO travel)
    {
        var existing = await _context.TravelPackages.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Pacote de viagem não encontrado para atualização.");

        _mapper.Map(travel, existing);
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return _mapper.Map<EditTravelPackageDTO>(existing);
    }
    
    public async Task<TravelPackage> DesativateAsync(int id)
    {
        var existing = await _context.TravelPackages.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Pacote de viagem não encontrado para desativação.");

        existing.IsActive = false;
        existing.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<TravelPackage> ActivateAsync(int id)
    {
        var existing = await _context.TravelPackages.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Pacote de viagem não encontrado para ativação.");

        existing.IsActive = true;
        existing.DeletedAt = null;

        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<TravelPackage> ActivePromotionAsync(int id, decimal discountPercentage)
    {
        var existing = await _context.TravelPackages.FindAsync(id);
        if (existing == null)
            throw new KeyNotFoundException("Pacote de viagem não encontrado para ativação de promoção.");

        existing.IsActive = true;
        if (discountPercentage > 0 && discountPercentage < 100)
        {
            var desconto = existing.Price * (discountPercentage / 100);
            existing.Price -= desconto;
        }

        await _context.SaveChangesAsync();
        return existing;
    }
    public async Task<TravelPackage> GetActiveAsync()
    {
        return await _context.TravelPackages.FirstOrDefaultAsync(tp => tp.IsActive == true);

    }

}