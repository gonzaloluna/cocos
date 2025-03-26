namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Models;

    public class PortfolioCalculator : IPortfolioCalculator
    {
        public PortfolioCalculationContext Calculate(
            IEnumerable<Order> orders,
            IEnumerable<IOrderProcessingStrategy> strategies)
        {
            var context = new PortfolioCalculationContext();

            foreach (var order in orders)
            {
                var strategy = strategies.FirstOrDefault(s => s.AppliesTo(order));
                strategy?.Process(order, context);
            }

            return context;
        }
    }

}

