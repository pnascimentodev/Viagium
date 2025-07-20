using AutoMapper;
using Viagium.Models;

namespace Viagium.EntitiesDTO;

public class EntitiesMappingDTO : Profile
{
    public EntitiesMappingDTO()
    {
        CreateMap<User, UserDTO>();
        CreateMap<Hotel, HotelDTO>();
        CreateMap<Payment, PaymentDTO>();
        CreateMap<Reservation, ReservationDTO>();
        CreateMap<ReservationRoom, ReservationRoomDTO>();
        CreateMap<Review, ReviewDTO>();
        CreateMap<RoomType, RoomTypeDTO>();
        CreateMap<Traveler, TravelerDTO>();
        CreateMap<TravelPackage, CreateTravelPackageDTO>();
        CreateMap<TravelPackageHistory, TravelPackageHistoryDTO>();
        
    }
}