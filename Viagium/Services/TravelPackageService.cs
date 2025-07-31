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
            if (travelPackage.Price <= 0)
                throw new Exception("Preço atual inválido.");
            // Salva o preço antigo
            travelPackage.OriginalPrice = travelPackage.Price;
            // Atualiza o desconto manual
            travelPackage.ManualDiscountValue = discountPercentage;
            // Calcula o novo preço com o desconto manual
            decimal price = travelPackage.OriginalPrice;
            price -= (travelPackage.OriginalPrice * travelPackage.ManualDiscountValue / 100);
            travelPackage.Price = price > 0 ? price : 0;
            await _context.SaveChangesAsync();
            return await ListAllAsync();
        }

        public async Task<List<ResponseTravelPackageDTO>> DesactivateDiscountAsync(int travelPackageId)
        {
            var travelPackage = await _context.TravelPackages.FirstOrDefaultAsync(tp => tp.TravelPackageId == travelPackageId);
            if (travelPackage == null)
                throw new Exception("Pacote não encontrado.");
            if (travelPackage.ManualDiscountValue == 0)
                throw new Exception("Não há desconto manual ativo para desativar.");
            // Restaura o preço anterior salvo em OriginalPrice, se válido
            travelPackage.ManualDiscountValue = 0;
            if (travelPackage.OriginalPrice > 0)
                travelPackage.Price = travelPackage.OriginalPrice;
            await _context.SaveChangesAsync();
            return await ListAllAsync();
        }

        public async Task<ResponseTravelPackageDTO?> UpdateCupomAsync(int travelPackageId, string cupom, decimal discountValue)
        {
            return await _travelPackageRepository.UpdateCupomAsync(travelPackageId, cupom, discountValue);
        }

    }
}