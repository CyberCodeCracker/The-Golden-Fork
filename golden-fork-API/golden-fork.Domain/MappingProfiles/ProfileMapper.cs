using AutoMapper;
using golden_fork.core.DTOs;
using golden_fork.core.DTOs.Cart;
using golden_fork.core.DTOs.Kitchen;
using golden_fork.core.DTOs.Order;
using golden_fork.core.DTOs.Purchase;
using golden_fork.core.Entities.AppCart;
using golden_fork.core.Entities.AppUser;
using golden_fork.core.Entities.Kitchen;
using golden_fork.core.Entities.Menu;
using golden_fork.core.Entities.Purchase;
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
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Item.Description))   
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Item.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Item.ImageUrl))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item.Category.Name));

            CreateMap<ItemRequest, Item>().ReverseMap();
            CreateMap<ItemUpdate, Item>().ReverseMap();
            CreateMap<Item, ItemResponse>().ReverseMap();

            // In your MappingProfiles/KitchenProfile.cs or main profile
            CreateMap<CategoryRequest, Category>().ReverseMap();
            CreateMap<CategoryUpdate, Category>().ReverseMap();
            CreateMap<Category, CategoryResponse>().ReverseMap();

            CreateMap<Category, CategoryWithItemsResponse>()
                .ForMember(dest => dest.ItemCount, opt => opt.Ignore())  // Set manually in service
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
            // Cart → CartResponse (manual mapping needed because of nested data)
            CreateMap<Cart, CartResponse>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // calculated manually
                .ForMember(dest => dest.TotalItems, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore());

            // CartItem → CartItemResponse
            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Item.ImageUrl))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Item.Price));

            CreateMap<Order, OrderResponse>()
                        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
                        .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.OrderItems.Sum(oi => oi.Quantity)))
                        .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));


            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item.Name))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Item.ImageUrl));

            CreateMap<Payment, PaymentResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsPaid ? "Paid" : "Pending"));

            CreateMap<MenuItemRequest, Item>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.SpecialPrice, opt => opt.MapFrom(src => src.SpecialPrice))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.IsAvailable));
        }
    }
    
}
