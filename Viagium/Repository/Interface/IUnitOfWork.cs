
﻿namespace Viagium.Repository.Interface;

﻿using Viagium.Repository.Interface;

namespace Viagium.Repository;


public interface IUnitOfWork :  IDisposable
{
    ITravelPackageRepository TravelPackageRepository { get; }
    IUserRepository UserRepository { get; }
    IAffiliateRepository AffiliateRepository { get; }
    IAddressRepository AddressRepository { get; }
    Task<int> SaveAsync();
}