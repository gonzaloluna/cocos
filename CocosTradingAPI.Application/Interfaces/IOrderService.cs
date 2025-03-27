namespace CocosTradingAPI.Application.Interfaces
{
    using CocosTradingAPI.Application.DTOs;
    using System.Threading.Tasks;

    public interface IOrderService
    {
        Task<OrderResultDto> PlaceOrderAsync(OrderRequestDto request);
    }
}