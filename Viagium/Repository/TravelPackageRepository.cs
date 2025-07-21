using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;

namespace Viagium.Repository;

public class TravelPackageRepository : ITravelPackageRepository
{
    private readonly AppDbContext _context;

    public TravelPackageRepository(AppDbContext context)
    {
        _context = context;
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
    
    public async Task<TravelPackage> UpdateAsync(TravelPackage travel)
    {
        var existing = await _context.TravelPackages.FindAsync(travel.TravelPackagesId);
        if (existing == null)
            throw new KeyNotFoundException("Pacote de viagem não encontrado para atualização.");

        // Atualiza apenas os campos necessários
        existing.HotelId = travel.HotelId;
        existing.Title = travel.Title;
        existing.Description = travel.Description;
        existing.OriginAddressId = travel.OriginAddressId;
        existing.DestinationAddressId = travel.DestinationAddressId;
        existing.ImageUrl = travel.ImageUrl;
        existing.Duration = travel.Duration;
        existing.MaxPeople = travel.MaxPeople;
        existing.VehicleType = travel.VehicleType;
        existing.Price = travel.Price;
        existing.CreatedBy = travel.CreatedBy;
        existing.IsActive = travel.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
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
}