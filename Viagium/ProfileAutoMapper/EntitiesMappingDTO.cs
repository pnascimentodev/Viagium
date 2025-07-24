using AutoMapper;
using Viagium.Models;
using Viagium.EntitiesDTO;
using Viagium.EntitiesDTO.User;

namespace Viagium.EntitiesDTO;

public class EntitiesMappingDTO : Profile
{
    public EntitiesMappingDTO()
    {
        CreateMap<Address, AddressDTO>();
        CreateMap<Affiliate, AffiliateDTO>();
        CreateMap<Viagium.Models.User, UserDTO>();
        CreateMap<Hotel, HotelDTO>();
        CreateMap<Payment, PaymentDTO>();
        CreateMap<Reservation, ReservationDTO>();
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
        CreateMap<Affiliate, AffiliateListDTO>()
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels));
        CreateMap<Address, AddressListDTO>();
        CreateMap<Hotel, HotelDTO>();
        CreateMap<Models.User, UserEmailDTO>();
    }
}