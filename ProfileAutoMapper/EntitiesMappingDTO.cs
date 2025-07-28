using AutoMapper;
using Viagium.Models;
using Viagium.EntitiesDTO;

namespace Viagium.ProfileAutoMapper
{
    public class EntitiesMappingProfile : Profile
    {
        public EntitiesMappingProfile()
        {
            CreateMap<TravelPackage, EditTravelPackageDTO>();
            CreateMap<EditTravelPackageDTO, TravelPackage>()
                .ForMember(dest => dest.TravelPackagesId, opt => opt.Ignore());

            // Mapeamento para Affiliate e Address
            CreateMap<Affiliate, AffiliateDTO>()
                .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
            CreateMap<AffiliateDTO, Affiliate>();

            CreateMap<Address, AddressDTO>()
                .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => src.AddressId));
            CreateMap<AddressDTO, Address>();

            CreateMap<Affiliate, AffiliateListDTO>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Hotels, opt => opt.MapFrom(src => src.Hotels))
                .ForMember(dest => dest.HashPassword, opt => opt.Ignore());
            CreateMap<Address, AddressListDTO>();
            CreateMap<Hotel, HotelDTO>();
            CreateMap<Hotel, HotelWithAddressDTO>();
        }
    }
}
