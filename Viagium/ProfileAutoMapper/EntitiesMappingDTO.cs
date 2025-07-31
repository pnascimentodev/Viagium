using AutoMapper;
using Viagium.Models;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.User;
using Viagium.EntitiesDTO.Affiliate;
using Viagium.EntitiesDTO.ApiDTO;
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
                })));
            CreateMap<Affiliate, AffiliateDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels));
            CreateMap<Viagium.Models.User, UserDTO>();

            CreateMap<Payment, PaymentDTO>();
            CreateMap<Viagium.Models.Reservation, ReservationDTO>();
            CreateMap<UserCreateDTO, AsaasUserDTO>();
            CreateMap<ReservationRoom, ReservationRoomDTO>();
            CreateMap<Review, ReviewDTO>();
            CreateMap<Room, RoomDTO>();
            CreateMap<RoomType, RoomTypeDTO>();
            CreateMap<Traveler, TravelerDTO>();
            CreateMap<TravelPackage, TravelPackageDTO>();
            CreateMap<TravelPackageDTO, TravelPackage>();
            CreateMap<TravelPackage, CreateTravelPackageDTO>();
            CreateMap<CreateTravelPackageDTO, TravelPackage>()
                .ForMember(dest => dest.OriginAddress, opt => opt.MapFrom(src => src.OriginAddress))
                .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.DestinationAddress))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId));
            CreateMap<TravelPackageHistory, TravelPackageHistoryDTO>();
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
                })));
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
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels));
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
            CreateMap<Hotel, HotelDTO>();
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
                .ForMember(dest => dest.Reservation, opt => opt.MapFrom(src => src.Reservation))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
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
            
            
        }
    }
}
