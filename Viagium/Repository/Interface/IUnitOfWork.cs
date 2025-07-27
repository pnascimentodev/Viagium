namespace Viagium.Repository.Interface;

public interface IUnitOfWork : IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    IUserRepository UserRepository { get; }
    IAffiliateRepository AffiliateRepository { get; }
    IAddressRepository AddressRepository { get; }
    IAmenityRepository AmenityRepository { get; }
    IHotelRepository HotelRepository { get; }
    Task<int> SaveAsync();
}
