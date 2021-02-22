using AutoMapper;
using CityAPI.Dtos;
using CityAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityAPI.Helpers
{
    public class AutoMappersProfiles : Profile
    {
        public AutoMappersProfiles()
        {
            CreateMap<City, CityForListDto>()
                .ForMember(destionation => destionation.PhotoUrl, opt =>
                {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);

                });

            CreateMap<City, CityForDetailDto>();

            CreateMap<Photo, PhotoForCreationDto>();
            CreateMap<PhotoForReturnDto, Photo>();
        }
    }
}
