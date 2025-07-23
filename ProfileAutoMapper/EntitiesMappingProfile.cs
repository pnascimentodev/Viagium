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
            CreateMap<EditTravelPackageDTO, TravelPackage>();
        }
    }
}

