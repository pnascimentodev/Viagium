using Viagium.Data;
using Viagium.Repository.Interface;
using Viagium.Services;

namespace Viagium.Repository;

public class UnitOfWork: IUnitOfWork
{
    private readonly AppDbContext _context;
    public ITravelPackageRepository TravelPackageRepository { get; }
    public IUserRepository UserRepository { get; }
    public IAffiliateRepository AffiliateRepository { get; }
    public IAddressRepository AddressRepository { get; }
    public IReservationRepository ReservationRepository { get; }
    public IPaymentRepository PaymentRepository { get; }
    public IAmenityRepository? AmenityRepository { get; }
    public IHotelRepository? HotelRepository { get; }
    public IReviewRepository ReviewRepository { get; }
    public ITravelerRepository TravelerRepository { get; }

    public UnitOfWork(AppDbContext context, ITravelPackageRepository travelPackageRepository,
        IUserRepository userRepository, IAffiliateRepository affiliateRepository, IAddressRepository addressRepository,
        IReservationRepository reservationRepository, IPaymentRepository paymentRepository, IReviewRepository reviewRepository, IAmenityRepository amenityRepository, IHotelRepository hotelRepository, ITravelerRepository travelerRepository)
    {
        _context = context;
        TravelPackageRepository = travelPackageRepository;
        UserRepository = userRepository;
        AffiliateRepository = affiliateRepository;
        AddressRepository = addressRepository;
        ReservationRepository = reservationRepository;
        AmenityRepository = amenityRepository;
        HotelRepository = hotelRepository;
        PaymentRepository = paymentRepository;
        ReviewRepository = reviewRepository;
        TravelerRepository = travelerRepository;
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
        _context.Dispose();
    }
}