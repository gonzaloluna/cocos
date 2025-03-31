namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class SellLimitOrderStrategyWithTotalAmount : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMarketDataRepository _marketDataRepository;

        public SellLimitOrderStrategyWithTotalAmount(
            IOrderRepository orderRepository,
            IMarketDataRepository marketDataRepository)
        {
            _orderRepository = orderRepository;
            _marketDataRepository = marketDataRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.SELL && request.Type == OrderType.LIMIT && request.TotalAmount > 0;
        }

        public async Task<OrderResultDto> ExecuteAsync(OrderRequestDto request)
        {
            if(request.TotalAmount <= 0)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Total amount must be greater than zero"
                };
            }
            
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

            if (marketData.Close < request.Price)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Market price is lower than the limit price"
                };
            }
            
            var orders = await _orderRepository.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId);
            var stockQuantity = orders.Sum(o => o.Side == OrderSide.BUY ? o.Size : -o.Size);

            // Calcular la cantidad de acciones a vender usando TotalAmount
            var maxSize = (int)(request.TotalAmount / marketData.Close); // Redondeo hacia abajo

            if (stockQuantity < maxSize)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient shares to sell"
                };
            }

            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Price = marketData.Close,
                Size = maxSize,
                Status = OrderStatus.NEW,
                DateTime = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);

            return new OrderResultDto
            {
                Success = true,
                Status = OrderStatus.NEW,
                Message = "Limit sell order created"
            };
        }
    }
}
