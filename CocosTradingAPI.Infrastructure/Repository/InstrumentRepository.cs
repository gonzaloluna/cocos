using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Models;
using CocosTradingAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CocosTradingAPI.Infrastructure.Repositories
{
    public class InstrumentRepository : IInstrumentRepository
    {
        private readonly ApplicationDbContext _context;

        public InstrumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InstrumentDto>> SearchByTickerOrNameAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();

            query = query.ToLower();

            var joined = await (
                from instrument in _context.Instruments
                join market in _context.MarketData
                    on instrument.Id equals market.InstrumentId
                where EF.Functions.ILike(instrument.Ticker, $"%{query}%")
                   || EF.Functions.ILike(instrument.Name, $"%{query}%")
                select new
                {
                    instrument.Id,
                    instrument.Ticker,
                    instrument.Name,
                    Type = instrument.Type.ToString(),
                    market.Date,
                    market.Close,
                    market.PreviousClose
                })
                .ToListAsync(); // Traemos todo a memoria

            var grouped = joined
                .GroupBy(x => new { x.Id, x.Ticker, x.Name, x.Type })
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.Date).First();

                    return new InstrumentDto
                    {
                        Id = g.Key.Id,
                        Ticker = g.Key.Ticker,
                        Name = g.Key.Name,
                        Type = g.Key.Type,
                        Close = latest.Close,
                        PreviousClose = latest.PreviousClose,
                        DailyReturn = ((latest.Close - latest.PreviousClose) / latest.PreviousClose) * 100
                    };
                })
                .ToList();

            return grouped;
        }


    }
}
