using AutoMapper;
using Viagium.Models;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.User;
using Viagium.EntitiesDTO.Affiliate;

namespace Viagium.ProfileAutoMapper
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile() : base()
        {
            CreateMap<Address, AddressDTO>();
            CreateMap<Affiliate, AffiliateDTO>();
            CreateMap<Viagium.Models.User, UserDTO>();
            CreateMap<Hotel, HotelDTO>();
            CreateMap<Payment, PaymentDTO>();
            CreateMap<Viagium.Models.Reservation, ReservationDTO>();
            CreateMap<ReservationRoom, ReservationRoomDTO>();
            CreateMap<Review, ReviewDTO>();
            CreateMap<Room, RoomDTO>();
            CreateMap<RoomType, RoomTypeDTO>();
            CreateMap<Traveler, TravelerDTO>();
            CreateMap<TravelPackage, TravelPackageDTO>();
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
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.HotelAmenity.Select(ha => ha.Amenity)));
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
            CreateMap<Models.Affiliate, AffiliateDTO>();
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
                .ForMember(dest => dest.RoomTypeAmenities, opt => opt.Ignore());
            CreateMap<RoomTypeUpdateDTO, RoomType>();
            CreateMap<RoomType, RoomTypeDTO>()
                .ForMember(dest => dest.Amenities, opt => opt.MapFrom(src => src.RoomTypeAmenities.Select(rta => rta.Amenity)));
            CreateMap<Amenity, AmenityDTO>();
        }
    }
}