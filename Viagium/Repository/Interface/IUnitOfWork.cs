namespace Viagium.Repository;

public interface IUnitOfWork :  IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    Task<int> SaveAsync();
}