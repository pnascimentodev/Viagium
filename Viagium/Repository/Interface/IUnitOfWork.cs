namespace Viagium.Repository.Interface;

public interface IUnitOfWork :  IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    IUserRepository UserRepository { get; }
    IAffiliateRepository AffiliateRepository { get; }
    Task<int> SaveAsync();
}