using Viagium.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Viagium.Repository.Interface;

public interface IAmenityRepository
{
    Task<Amenity?> GetByIdAsync(int id);
    Task<List<Amenity>> GetByIdsAsync(IEnumerable<int> ids);
    Task<Amenity?> GetByNameAsync(string name);
    Task<List<Amenity>> GetAllAsync();
}
