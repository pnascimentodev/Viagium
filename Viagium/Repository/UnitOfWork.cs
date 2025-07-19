using Viagium.Data;

namespace Viagium.Repository;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITravelPackageRepository TravelPackageRepository { get; }
    public UnitOfWork(AppDbContext context, ITravelPackageRepository travelPackageRepository)
    {
        _context = context;
        TravelPackageRepository = travelPackageRepository;
    }
    public void Dispose()
    {
        throw new NotImplementedException();
    }
    
    public Task<int> SaveAsync()
    {
        throw new NotImplementedException();
    }
}