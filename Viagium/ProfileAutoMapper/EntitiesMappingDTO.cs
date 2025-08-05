using AutoMapper;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.Affiliate;
using Viagium.EntitiesDTO.ApiDTO;
using Viagium.EntitiesDTO.Reservation;
using Viagium.EntitiesDTO.User;
using Viagium.Models;
using Viagium.EntitiesDTO.TravelPackageDTO;

namespace Viagium.ProfileAutoMapper
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile() : base()
        {
            CreateMap<Address, AddressDTO>();
            CreateMap<Hotel, HotelWithAddressDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.HotelAmenity.Select(ha => new AmenityDTO
                {
                    AmenityId = ha.Amenity.AmenityId,
                    Name = ha.Amenity.Name,
                    IconName = ha.Amenity.IconName
                })))
                .ForMember(dest => dest.RoomTypes, opt => opt.MapFrom(src => src.RoomTypes))
                .ForMember(dest => dest.Affiliate, opt => opt.MapFrom(src => src.Affiliate));
            CreateMap<Affiliate, AffiliateDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
            CreateMap<Address, AddressDTO>();
            CreateMap<Viagium.Models.User, UserDTO>();

            CreateMap<Payment, PaymentDTO>();
            CreateMap<Viagium.Models.Reservation, ReservationDTO>();
            CreateMap<Reservation, CreateReservationDTO>().ReverseMap()
                .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId)) // Mapear o HotelId
                .ForMember(dest => dest.RoomTypeId, opt => opt.MapFrom(src => src.RoomTypeId)); // Mapear o RoomTypeId
            CreateMap<UserCreateDTO, AsaasUserDTO>();
            CreateMap<ReservationRoom, ReservationRoomDTO>();
            CreateMap<CreateReservationDTO, ResponseReservationDTO>().ReverseMap();
            CreateMap<Reservation,ResponseReservationDTO>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Travelers, opt => opt.Ignore()) // Travelers serão preenchidos manualmente no serviço
                .ForMember(dest => dest.TravelPackage, opt => opt.MapFrom(src => src.TravelPackage))
                .ForMember(dest => dest.Hotel, opt => opt.MapFrom(src => src.Hotel)) // Agora mapeamos o Hotel diretamente
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType ?? (src.ReservationRooms != null && src.ReservationRooms.Any() ? src.ReservationRooms.First().RoomType : null))); // Priorizar RoomType direto, depois ReservationRooms
            CreateMap<Review, ReviewDTO>();
            CreateMap<Room, RoomDTO>();
            CreateMap<RoomType, RoomTypeDTO>();
            CreateMap<Traveler, TravelerDTO>();
            CreateMap<TravelerDTO, Traveler>(); // Adicionando mapeamento reverso
            CreateMap<TravelPackage, TravelPackageDTO>();
            CreateMap<TravelPackageDTO, TravelPackage>();
            CreateMap<TravelPackage, CreateTravelPackageDTO>();
            CreateMap<CreateTravelPackageDTO, TravelPackage>()
                .ForMember(dest => dest.OriginAddress, opt => opt.MapFrom(src => src.OriginAddress))
                .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.DestinationAddress))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId));
            CreateMap<TravelPackageHistory, TravelPackageHistoryDTO>();
            CreateMap<ResponseTravelPackageDTO, UpdateTravelPackageFormDTO>().ReverseMap();
            CreateMap<UpdateTravelPackageFormDTO, ResponseTravelPackageDTO>()
                .ForMember(dest => dest.Hotels, opt => opt.Ignore())
                .ForMember(dest => dest.PackageSchedule, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<UserUpdateDto, Viagium.Models.User>()
                .ForMember(dest => dest.DocumentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<UserCreateDTO, Viagium.Models.User>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<Address, AddressDTO>().ReverseMap();
            CreateMap<UserDTO, UserListDTO>();
            CreateMap<AdminRegisterDTO, Viagium.Models.User>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<AdminUpdateDTO, Viagium.Models.User>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<Viagium.Models.User, AdminDTO>();

            CreateMap<Hotel, HotelWithAddressDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.HotelAmenity.Select(ha => new AmenityDTO
                {
                    AmenityId = ha.Amenity.AmenityId,
                    Name = ha.Amenity.Name,
                    IconName = ha.Amenity.IconName
                })))
                .ForMember(dest => dest.RoomTypes, opt => opt.MapFrom(src => src.RoomTypes));
            CreateMap<HotelWithAddressDTO, Hotel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
            CreateMap<HotelCreateFormDTO, Hotel>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()) // será preenchido após upload
                .ForMember(dest => dest.HotelId, opt => opt.Ignore()) // gerado pelo banco
                .ForMember(dest => dest.Affiliate, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.HotelAmenity, opt => opt.Ignore());
            // Mapeamentos para Affiliate
            CreateMap<AffiliateCreateDto, Models.Affiliate>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.AffiliateId, opt => opt.Ignore());
            CreateMap<Models.Affiliate, AffiliateDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
            CreateMap<Models.Affiliate, AffiliateListDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels));

            // Mapeamentos para User
            CreateMap<UserUpdateDto, Viagium.Models.User>()
                .ForMember(dest => dest.DocumentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<UserCreateDTO, Viagium.Models.User>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<UserDTO, UserListDTO>();
            CreateMap<Models.User, UserEmailDTO>();

            // Mapeamentos para Admin
            CreateMap<AdminRegisterDTO, Viagium.Models.User>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<AdminUpdateDTO, Viagium.Models.User>()
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
            CreateMap<Viagium.Models.User, AdminDTO>();

            // Mapeamentos para Address
            CreateMap<Address, AddressDTO>().ReverseMap();
            CreateMap<Address, AddressListDTO>();
            CreateMap<Hotel, HotelDTO>()
                .ForMember(dest => dest.Affiliate, opt => opt.MapFrom(src => src.Affiliate))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            CreateMap<HotelWithAddressDTO, HotelDTO>()
                .ForMember(dest => dest.AffiliateId, opt => opt.MapFrom(src => src.AffiliateId))
                .ForMember(dest => dest.AddressId, opt => opt.Ignore()) // HotelWithAddressDTO não tem AddressId
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
            CreateMap<Models.User, UserEmailDTO>();
            CreateMap<RoomTypeCreateDTO, RoomType>()
                    //.ForMember(dest => dest.RoomTypeAmenities, opt => opt.Ignore());   verificar com priscilla
                    .ForMember(dest => dest.RoomTypeAmenities, opt => opt.Ignore()) // Preenchido manualmente no service
                    .ForMember(dest => dest.Rooms, opt => opt.Ignore()) //  ADICIONAR - Preenchido manualmente no service
                    .ForMember(dest => dest.RoomTypeId, opt => opt.Ignore()) // Gerado pelo banco
                    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Será definido automaticamente
                    .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Será definido manualmente
                    .ForMember(dest => dest.DeletedAt, opt => opt.Ignore()) // Não deve ser mapeado
                    .ForMember(dest => dest.Hotel, opt => opt.Ignore()); // Navegação será resolvida pelo EF
            CreateMap<RoomTypeUpdateDTO, RoomType>();
            CreateMap<RoomType, RoomTypeDTO>()
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.RoomTypeAmenities.Select(rta => rta.Amenity)));
            CreateMap<Amenity, AmenityDTO>();

            // Mapeamentos para Review
            CreateMap<Review, ReviewDTO>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
            // ✅ MAPEAMENTO REVERSO ADICIONADO: ReviewDTO -> Review
            CreateMap<ReviewDTO, Review>()
                .ForMember(dest => dest.ReviewId, opt => opt.Ignore()) // Será gerado pelo banco
                .ForMember(dest => dest.Reservation, opt => opt.Ignore()) // Navegação será resolvida pelo EF
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now)); // Define data atual

            CreateMap<AddressPackageDTO, Address>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            CreateMap<Address, AddressPackageDTO>()
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            CreateMap<CreateTravelPackageDTO, TravelPackage>()
                .ForMember(dest => dest.OriginAddress, opt => opt.MapFrom(src => src.OriginAddress))
                .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.DestinationAddress))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId));
            CreateMap<PackageSchedule, PackageScheduleDTO>();
            CreateMap<UserUpdateDto, UserDTO>()
                .ForMember(dest => dest.DocumentNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
