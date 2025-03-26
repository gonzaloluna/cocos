using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Models;
using CocosTradingAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CocosTradingAPI.Infrastructure.Repositories
{
    public class MarketDataRepository : IMarketDataRepository
    {
        private readonly ApplicationDbContext _context;

        public MarketDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MarketData?> GetLatestForInstrumentAsync(int instrumentId)
        {
            return await _context.MarketData
                .Where(m => m.InstrumentId == instrumentId)
                .OrderByDescending(m => m.Date)
                .FirstOrDefaultAsync();
        }
    }
}
