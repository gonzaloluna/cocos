namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Models;
    using Microsoft.Extensions.Logging;

    public class PortfolioService : IPortfolioService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMarketDataRepository _marketRepo;
        private readonly IPortfolioCalculator _portfolioCalculator;
        private readonly ILoggerFactory _loggerFactory;

        public PortfolioService(
            IOrderRepository orderRepo,
            IMarketDataRepository marketRepo,
            IPortfolioCalculator portfolioCalculator,
            ILoggerFactory loggerFactory)
        {
            _orderRepo = orderRepo;
            _marketRepo = marketRepo;
            _portfolioCalculator = portfolioCalculator;
            _loggerFactory = loggerFactory;
        }

        public async Task<PortfolioDto> GetPortfolioAsync(int userId)
        {
            var orders = await _orderRepo.GetFilledOrdersByUserAsync(userId);

            var instrumentIds = orders.Select(o => o.InstrumentId).Distinct().ToList();
            var marketDataList = await _marketRepo.GetLatestForInstrumentsAsync(instrumentIds);
            var marketDataDict = marketDataList.ToDictionary(m => m.InstrumentId);

            var strategyBuilder = new OrderProcessingStrategyBuilder(marketDataDict, _loggerFactory);
            var strategies = strategyBuilder.Build();

            var context = _portfolioCalculator.Calculate(orders, strategies);

            return new PortfolioDto
            {
                AvailableCash = context.AvailableCash,
                Positions = context.Positions.Values.ToList(),
                TotalValue = context.AvailableCash + context.Positions.Values.Sum(p => p.TotalValue)
            };
        }
    }
}