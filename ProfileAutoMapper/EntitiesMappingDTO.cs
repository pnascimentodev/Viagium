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
        }
    }
}
