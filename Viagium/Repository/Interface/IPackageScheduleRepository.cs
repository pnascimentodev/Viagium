using Viagium.Models;

namespace Viagium.Repository.Interface;

public interface IPackageScheduleRepository
{
    Task<IEnumerable<PackageSchedule>> GetByTravelPackageIdAsync(int travelPackageId);
    Task<PackageSchedule?> GetByIdAsync(int id);
    Task AddAsync(PackageSchedule packageSchedule);
    Task UpdateAsync(PackageSchedule packageSchedule);
    Task DeleteAsync(int id);
    Task<IEnumerable<PackageSchedule>> GetAllAsync();
}
