namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class BuyMarketOrderStrategy : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMarketDataRepository _marketDataRepository;
        private readonly IInstrumentRepository _instrumentRepository;

        public BuyMarketOrderStrategy(
            IOrderRepository orderRepository,
            IMarketDataRepository marketDataRepository,
            IInstrumentRepository instrumentRepository)
        {
            _orderRepository = orderRepository;
            _marketDataRepository = marketDataRepository;
            _instrumentRepository = instrumentRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.BUY && request.Type == OrderType.MARKET;
        }

        public async Task<OrderResultDto> ExecuteAsync(OrderRequestDto request)
        {
            var cashAvailable = await _orderRepository.GetAvailableCashAsync(request.UserId);
            var marketDataList = await _marketDataRepository.GetLatestForInstrumentsAsync(new[] { request.InstrumentId });
            var marketData = marketDataList.FirstOrDefault();

            if (marketData == null || marketData.Close <= 0)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "No market data available for the selected instrument"
                };
            }

            var marketPrice = marketData.Close;

            var totalCost = marketPrice * request.Size;

            if (cashAvailable < totalCost)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient funds"
                };
            }

            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Price = marketPrice,
                Size = request.Size,
                Status = OrderStatus.FILLED,
                DateTime = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);

            return new OrderResultDto
            {
                Success = true,
                Status = OrderStatus.FILLED,
                Message = "Market buy order executed"
            };
        }
    }
}
