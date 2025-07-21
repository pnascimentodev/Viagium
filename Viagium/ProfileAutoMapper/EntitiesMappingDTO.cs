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
        CreateMap<CreateTravelPackageDTO, TravelPackage>()
            .ForMember(dest => dest.Hotel, opt => opt.Ignore())
            .ForMember(dest => dest.OriginAddress, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddress, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
        CreateMap<TravelPackageHistory, TravelPackageHistoryDTO>();
    }
}