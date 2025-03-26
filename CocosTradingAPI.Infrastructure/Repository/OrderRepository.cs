using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Enums;
using CocosTradingAPI.Domain.Models;
using CocosTradingAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CocosTradingAPI.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetFilledOrdersByUserAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Instrument)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.FILLED)
                .ToListAsync();
        }
    }
}
