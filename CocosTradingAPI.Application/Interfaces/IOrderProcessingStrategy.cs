namespace CocosTradingAPI.Application.Interfaces
{
    using CocosTradingAPI.Domain.Models;
    using CocosTradingAPI.Application.Services;

    public interface IOrderProcessingStrategy
    {
        bool AppliesTo(Order order);
        void Process(Order order, PortfolioCalculationContext context);
    }
}