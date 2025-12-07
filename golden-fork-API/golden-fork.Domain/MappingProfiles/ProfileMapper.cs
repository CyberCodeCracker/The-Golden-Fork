using AutoMapper;
using golden_fork.core.DTOs;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.Entities.AppUser;
using golden_fork.core.Entities.Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.MappingProfiles
{
    public class ProfileMapper : Profile
    {
        public ProfileMapper()
        {
            CreateMap<RegistrationRequest, User>();
            CreateMap<MenuRequest, Menu>();
            // Menu to MenuCardResponse
            CreateMap<Menu, MenuCardResponse>()
                .ForMember(dest => dest.ItemCount,
                    opt => opt.MapFrom(src => src.MenuItems.Count));

            // Menu to MenuWithItemsResponse
            CreateMap<Menu, MenuWithItemsResponse>()
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.MenuItems
                        .Where(mi => mi.IsAvailable && mi.Item.IsAvailable)
                        .OrderBy(mi => mi.Position)));

            // MenuItem to MenuItemResponse
            CreateMap<MenuItem, MenuItemResponse>()
                .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src => src.ItemId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Item.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Item.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Item.ImageUrl))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item.Category.Name));
        }
    }
}
