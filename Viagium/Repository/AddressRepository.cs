using Microsoft.EntityFrameworkCore;
using Viagium.Data;
using Viagium.Models;
using Viagium.Repository.Interface;

namespace Viagium.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }



        public async Task AddAsync(Address address)
        {
            await _context.Addresses.AddAsync(address);
        }

        public async Task<IEnumerable<Address>> GetAllAsync()
        {
            return await _context.Addresses.ToListAsync();
        }

        public async Task<Address?> GetByIdAsync(int id)
        {
            return await _context.Addresses.FindAsync(id);
        }

        public async Task UpdateAsync(Address address)
        {
            var existingAddress = await _context.Addresses.FindAsync(address.AdressId);
            if (existingAddress != null)
            {
                _context.Entry(existingAddress).CurrentValues.SetValues(address);
            }
        }
    }
}
