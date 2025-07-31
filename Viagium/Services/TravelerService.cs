using Viagium.Models;
using Viagium.Repository.Interface;
using Viagium.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Viagium.Services
{
    public class TravelerService : ITravelerService
    {
        private readonly ITravelerRepository _travelerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TravelerService(ITravelerRepository travelerRepository, IUnitOfWork unitOfWork)
        {
            _travelerRepository = travelerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task AddAsync(Traveler traveler)
        {
            await _travelerRepository.AddAsync(traveler);
        }

        public async Task<IEnumerable<Traveler>> GetAllAsync()
        {
            return await _travelerRepository.GetAllAsync();
        }

        public async Task<Traveler?> GetByIdAsync(int id)
        {
            return await _travelerRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Traveler>> GetByReservationIdAsync(int reservationId)
        {
            return await _travelerRepository.GetByReservationIdAsync(reservationId);
        }
    }
}
