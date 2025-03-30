namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class BuyLimitOrderStrategyWithTotalAmount : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMarketDataRepository _marketDataRepository;

        public BuyLimitOrderStrategyWithTotalAmount(
            IOrderRepository orderRepository,
            IMarketDataRepository marketDataRepository)
        {
            _orderRepository = orderRepository;
            _marketDataRepository = marketDataRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.BUY && request.Type == OrderType.LIMIT && request.TotalAmount > 0;
        }

        public async Task<OrderResultDto> ExecuteAsync(OrderRequestDto request)
        {
            var availableCash = await _orderRepository.GetAvailableCashAsync(request.UserId);

            var marketDataList = await _marketDataRepository.GetLatestForInstrumentsAsync(new[] { request.InstrumentId });
            var marketData = marketDataList.FirstOrDefault();

            if (marketData == null || marketData.Close > request.Price)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Market price is higher than the limit price"
                };
            }

            if(request.TotalAmount <= 0)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Total amount must be greater than zero"
                };
            }
            // Calcular la cantidad de acciones a comprar usando TotalAmount
            var maxActions = (int)(request.TotalAmount / marketData.Close); 
            var totalCost = maxActions * marketData.Close;

            if (availableCash < totalCost)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient funds to buy the requested shares"
                };
            }

            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Price = marketData.Close,
                Size = maxActions,
                Status = OrderStatus.NEW, 
                DateTime = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);

            return new OrderResultDto
            {
                Success = true,
                Status = OrderStatus.NEW,
                Message = "Limit buy order created"
            };
        }
    }
}
