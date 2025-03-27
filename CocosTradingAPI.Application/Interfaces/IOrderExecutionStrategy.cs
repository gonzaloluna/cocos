namespace CocosTradingAPI.Application.Interfaces
{
    using CocosTradingAPI.Application.DTOs;
    using System.Threading.Tasks;

    public interface IOrderExecutionStrategy
    {
        bool AppliesTo(OrderRequestDto request);
        Task<OrderResultDto> ExecuteAsync(OrderRequestDto request);
    }
}