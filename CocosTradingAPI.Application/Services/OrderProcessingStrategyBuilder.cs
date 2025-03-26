namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Models;
    using Microsoft.Extensions.Logging;

    public class OrderProcessingStrategyBuilder
    {
        private readonly IDictionary<int, MarketData> _marketData;
        private readonly ILoggerFactory _loggerFactory;

        public OrderProcessingStrategyBuilder(
           IDictionary<int, MarketData> marketData,
           ILoggerFactory loggerFactory)
        {
            _marketData = marketData;
            _loggerFactory = loggerFactory;
        }

        public IEnumerable<IOrderProcessingStrategy> Build()
        {
            return new List<IOrderProcessingStrategy>
            {
                new CashOrderStrategy(), 
                new StockOrderStrategy(
                    _marketData,
                    _loggerFactory.CreateLogger<StockOrderStrategy>())
            };
        }
    }
}
