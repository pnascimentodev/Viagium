namespace Viagium.Repository;

public interface IUnitOfWork :  IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    IUserRepository UserRepository { get; }
    Task<int> SaveAsync();
}