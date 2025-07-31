using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.TravelPackageDTO;
using Viagium.Models;
using Viagium.Repository;
using Viagium.Repository.Interface;
using Viagium.Data;
using Microsoft.EntityFrameworkCore;

namespace Viagium.Services
{
    public class TravelPackageService : ITravelPackage
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITravelPackageRepository _travelPackageRepository;
        private readonly IHotelRepository _hotelRepository;
        private readonly AppDbContext _context;

        public TravelPackageService(IUnitOfWork unitOfWork, IMapper mapper, ITravelPackageRepository travelPackageRepository, IHotelRepository hotelRepository, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _travelPackageRepository = travelPackageRepository;
            _hotelRepository = hotelRepository;
            _context = context;
        }
        
        public async Task<ResponseTravelPackageDTO> AddAsync(CreateTravelPackageDTO createTravelPackageDto)
        {
            // Salva o pacote de viagem e obtém o objeto salvo (com hotéis do destino)
            var response = await _travelPackageRepository.AddAsync(createTravelPackageDto);

            // Busca o pacote salvo para obter o TravelPackageId
            var travelPackage = await _context.TravelPackages
                .Include(tp => tp.DestinationAddress)
                .FirstOrDefaultAsync(tp => tp.TravelPackageId == response.TravelPackageId);

            // Após salvar, associa hotéis ativos do destino ao pacote (relacionamento muitos-para-muitos)
            if (travelPackage?.DestinationAddress != null)
            {
                var city = travelPackage.DestinationAddress.City;
                var country = travelPackage.DestinationAddress.Country;
                await _travelPackageRepository.AssociateActiveHotelsByCityAndCountry(
                    travelPackageId: travelPackage.TravelPackageId,
                    city: city,
                    country: country
                );

                // Buscar hotéis novamente após associação
                var hotels = await _context.Hotels
                    .Include(h => h.Address)
                    .Where(h => h.IsActive &&
                                h.Address.City.ToLower() == city.ToLower() &&
                                h.Address.Country.ToLower() == country.ToLower())
                    .ToListAsync();
                response.Hotels = _mapper.Map<List<HotelDTO>>(hotels);
            }
            return response;
        }


    }
}