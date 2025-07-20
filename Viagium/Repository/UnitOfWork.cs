using Viagium.Data;
using Viagium.Services;

namespace Viagium.Repository;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITravelPackageRepository TravelPackageRepository { get; }
    public IUserRepository UserRepository { get; }

    public UnitOfWork(AppDbContext context, ITravelPackageRepository travelPackageRepository, IUserRepository userRepository)
    {
        _context = context;
        TravelPackageRepository = travelPackageRepository;
        UserRepository = userRepository;
    }

    
    public async Task<int> SaveAsync()
    {
        return await ExceptionHandler.ExecuteWithHandling(
            async () => await _context.SaveChangesAsync(),
            "Salvamento no banco de dados"
        );
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}