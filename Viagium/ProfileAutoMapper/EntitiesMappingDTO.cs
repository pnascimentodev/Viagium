using AutoMapper;
using Viagium.Models;
using Viagium.EntitiesDTO;

namespace Viagium.EntitiesDTO;

public class EntitiesMappingDTO : Profile
{
    public EntitiesMappingDTO()
    {
        CreateMap<Address, AddressDTO>().ReverseMap();
        CreateMap<Affiliate, AffiliateDTO>().ReverseMap();
        CreateMap<User, UserDTO>().ReverseMap();
        CreateMap<Hotel, HotelDTO>().ReverseMap();
        CreateMap<Payment, PaymentDTO>().ReverseMap();
        CreateMap<Reservation, ReservationDTO>().ReverseMap();
        CreateMap<ReservationRoom, ReservationRoomDTO>().ReverseMap();
        CreateMap<Review, ReviewDTO>().ReverseMap();
        CreateMap<Room, RoomDTO>().ReverseMap();
        CreateMap<RoomType, RoomTypeDTO>().ReverseMap();
        CreateMap<Traveler, TravelerDTO>().ReverseMap();
        CreateMap<TravelPackage, TravelPackageDTO>().ReverseMap();
        CreateMap<TravelPackageHistory, TravelPackageHistoryDTO>().ReverseMap();
    }
}