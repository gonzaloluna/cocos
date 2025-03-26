using CocosTradingAPI.Application.DTOs;

namespace CocosTradingAPI.Application.Interfaces
{
    public interface IInstrumentRepository
    {
        Task<List<InstrumentDto>> SearchByTickerOrNameAsync(string query);
    }
}
