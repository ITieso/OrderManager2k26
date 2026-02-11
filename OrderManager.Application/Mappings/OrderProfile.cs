using AutoMapper;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Entities;

namespace OrderManager.Application.Mappings;

/// <summary>
/// AutoMapper profile for Order entity mappings.
/// </summary>
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<OrderItem, OrderItemResponse>();
    }
}
