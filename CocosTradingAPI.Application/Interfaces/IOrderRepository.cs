using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetFilledOrdersByUserAsync(int userId);
    }
}
