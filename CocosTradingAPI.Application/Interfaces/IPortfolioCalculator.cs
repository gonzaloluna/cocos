using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Domain.Models;

public interface IPortfolioCalculator
{
    PortfolioCalculationContext Calculate(
        IEnumerable<Order> orders,
        IEnumerable<IOrderProcessingStrategy> strategies);
}
