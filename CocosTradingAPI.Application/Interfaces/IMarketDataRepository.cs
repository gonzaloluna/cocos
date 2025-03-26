using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Application.Interfaces
{
    public interface IMarketDataRepository
    {
        Task<MarketData?> GetLatestForInstrumentAsync(int instrumentId);
    }
}
