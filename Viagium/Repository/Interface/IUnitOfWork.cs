namespace Viagium.Repository.Interface;

public interface IUnitOfWork : IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    IUserRepository UserRepository { get; }
    IAffiliateRepository AffiliateRepository { get; }
    IAddressRepository AddressRepository { get; }
    IReservationRepository ReservationRepository { get; }
    IAmenityRepository AmenityRepository { get; }
    IHotelRepository HotelRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IReviewRepository ReviewRepository { get; }
    ITravelerRepository TravelerRepository { get; }
    IRoomTypeRepository RoomTypeRepository { get; }
    IRoomRepository RoomRepository { get; }
    IPackageScheduleRepository PackageScheduleRepository { get; }
    Task<int> SaveAsync();
}
