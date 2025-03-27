using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetFilledOrdersByUserAsync(int userId);
        Task<List<Order>> GetFilledOrdersByUserAndInstrumentAsync(int userId, int instrumentId);
        Task<int> GetAvailableCashAsync(int userId);
        Task AddAsync(Order order);
    }
}
