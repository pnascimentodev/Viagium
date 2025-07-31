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

        public async Task<List<ResponseTravelPackageDTO>> ListAllAsync()
        {
            return await _travelPackageRepository.ListAllAsync();
        }

        public async Task<ResponseTravelPackageDTO?> UpdateAsync(ResponseTravelPackageDTO responseTravelPackageDTO)
        {
            var hasReservation = await _context.Reservations.AnyAsync(r => r.TravelPackageId == responseTravelPackageDTO.TravelPackageId);
            if (hasReservation)
                throw new Exception("Não é possível atualizar um pacote com reservas.");
            return await _travelPackageRepository.UpdateAsync(responseTravelPackageDTO);
        }

        public async Task<CreateTravelPackageDTO?> AssociateActiveHotelsByCityAndCountry(int travelPackageId, string city, string country)
        {
            return await _travelPackageRepository.AssociateActiveHotelsByCityAndCountry(travelPackageId, city, country);
        }

        public async Task<ResponseTravelPackageDTO?> GetByIdAsync(int id)
        {
            return await _travelPackageRepository.GetByIdAsync(id);
        }

        public async Task<ResponseTravelPackageDTO?> GetByNameAsync(string name)
        {
            return await _travelPackageRepository.GetByNameAsync(name);
        }

        public async Task<ResponseTravelPackageDTO?> GetByCityAndCountryAsync(string city, string country)
        {
            return await _travelPackageRepository.GetByCityAndCountryAsync(city, country);
        }

        public async Task<List<ResponseTravelPackageDTO>> DesactivateAsync(int id)
        {
            var hasActiveReservation = await _context.Reservations.AnyAsync(r => r.TravelPackageId == id && r.IsActive);
            if (hasActiveReservation)
                throw new Exception("Não é possível desativar um pacote com reservas ativas.");
            return await _travelPackageRepository.DesactivateAsync(id);
        }

        public async Task<List<ResponseTravelPackageDTO>> ActivateAsync(int id)
        {
            return await _travelPackageRepository.ActivateAsync(id);
        }

        public async Task<List<ResponseTravelPackageDTO>> CreateDiscountAsync(int travelPackageId, decimal discountPercentage, DateTime startDate, DateTime endDate)
        {
            var travelPackage = await _context.TravelPackages.FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);
            if (travelPackage == null)
                throw new Exception("Pacote não encontrado.");
            if (discountPercentage <= 0 || discountPercentage > 100)
                throw new Exception("Desconto inválido.");
            if (travelPackage.OriginalPrice <= 0)
                throw new Exception("Preço original inválido.");
            if (travelPackage.Price != null)
                throw new Exception("Já existe desconto ativo neste pacote.");

            // Regra: Price recebe o valor antigo, OriginalPrice recebe o valor com desconto
            travelPackage.Price = travelPackage.OriginalPrice;
            var desconto = (travelPackage.OriginalPrice * discountPercentage) / 100;
            travelPackage.OriginalPrice = travelPackage.OriginalPrice - desconto;
            travelPackage.DiscountValue = discountPercentage;
            await _context.SaveChangesAsync();
            return await ListAllAsync();
        }

        public async Task<List<ResponseTravelPackageDTO>> DesactivateDiscountAsync(int travelPackageId)
        {
            var travelPackage = await _context.TravelPackages.FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);
            if (travelPackage == null)
                throw new Exception("Pacote não encontrado.");
            if (travelPackage.Price == null)
                throw new Exception("Não há desconto ativo para desativar.");
            // Regra: OriginalPrice recebe o valor de Price, Price volta a ser null
            travelPackage.OriginalPrice = travelPackage.Price;
            travelPackage.Price = 0;
            travelPackage.DiscountValue = 0;
            await _context.SaveChangesAsync();
            return await ListAllAsync();
        }

    }
}