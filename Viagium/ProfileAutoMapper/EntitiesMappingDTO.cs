using AutoMapper;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class EntitiesMappingDTO : Profile
{
    public EntitiesMappingDTO()
    {
        CreateMap<Address, AddressDTO>();
        CreateMap<Affiliate, AffiliateDTO>();
        CreateMap<User, UserDTO>();
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
        CreateMap<UserUpdateDTO, User>()
            .ForMember(dest => dest.DocumentNumber, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        CreateMap<UserCreateDTO, User>()
            .ForMember(dest => dest.HashPassword, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}