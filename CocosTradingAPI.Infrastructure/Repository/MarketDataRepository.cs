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

        public async Task<List<MarketData>> GetLatestForInstrumentsAsync(IEnumerable<int> instrumentIds)
        {
            return await _context.MarketData
                .Where(md => instrumentIds.Contains(md.InstrumentId))
                .GroupBy(md => md.InstrumentId)
                .Select(g => g.OrderByDescending(md => md.Date).First())
                .ToListAsync();
        }
    }
}
