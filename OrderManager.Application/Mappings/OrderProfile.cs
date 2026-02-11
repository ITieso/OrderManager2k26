using AutoMapper;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Entities;

namespace OrderManager.Application.Mappings;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderItem, OrderItemResponse>();
    }
}
