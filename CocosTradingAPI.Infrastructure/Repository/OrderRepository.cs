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
                .Where(o => o.UserId == userId && o.Status == OrderStatus.FILLED).OrderBy(o => o.DateTime)
                .ToListAsync();
        }

        public async Task<List<Order>> GetFilledOrdersByUserAndInstrumentAsync(int userId, int instrumentId)
        {
            return await _context.Orders
                .Include(o => o.Instrument)
                .Where(o => o.UserId == userId && o.InstrumentId == instrumentId && o.Status == OrderStatus.FILLED)
                .ToListAsync();
        }

        public async Task<int> GetAvailableCashAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.Instrument)
                .Where(o => o.UserId == userId &&
                            o.Status == OrderStatus.FILLED &&
                            o.Instrument.Type == InstrumentType.MONEDA)
                .SumAsync(o => o.Side == OrderSide.CASH_IN ? o.Size : -o.Size);
        }
        
        public async Task AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
    }
}
