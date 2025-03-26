using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Application.Interfaces
{
    public interface IMarketDataRepository
    {
        Task<List<MarketData>> GetLatestForInstrumentsAsync(IEnumerable<int> instrumentIds);
    }
}
