using Viagium.Repository.Interface;

namespace Viagium.Repository;

public interface IUnitOfWork :  IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    IUserRepository UserRepository { get; }
    IAddressRepository AddressRepository { get; }
    Task<int> SaveAsync();
}