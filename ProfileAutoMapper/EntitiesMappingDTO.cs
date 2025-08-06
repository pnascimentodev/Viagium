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

            CreateMap<TravelPackage, ResponseTravelPackageDTO>()
                .ForMember(dest => dest.OriginCity, opt => opt.MapFrom(src => src.OriginAddress.City))
                .ForMember(dest => dest.OriginCountry, opt => opt.MapFrom(src => src.OriginAddress.Country))
                .ForMember(dest => dest.DestinationCity, opt => opt.MapFrom(src => src.DestinationAddress.City))
                .ForMember(dest => dest.DestinationCountry, opt => opt.MapFrom(src => src.DestinationAddress.Country))
                .ForMember(dest => dest.ManualDiscountValue, opt => opt.MapFrom(src => src.ManualDiscountValue))
                .ReverseMap();

            CreateMap<CreateTravelPackageDTO, TravelPackage>()
                .ForMember(dest => dest.OriginAddress, opt => opt.MapFrom(src => src.OriginAddress))
                .ForMember(dest => dest.DestinationAddress, opt => opt.MapFrom(src => src.DestinationAddress))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ManualDiscountValue, opt => opt.MapFrom(src => src.ManualDiscountValue));
        }
    }
}
