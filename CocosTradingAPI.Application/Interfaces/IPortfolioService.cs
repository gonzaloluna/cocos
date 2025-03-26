using CocosTradingAPI.Application.DTOs;

namespace CocosTradingAPI.Application.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioDto> GetPortfolioAsync(int userId);
    }
}
