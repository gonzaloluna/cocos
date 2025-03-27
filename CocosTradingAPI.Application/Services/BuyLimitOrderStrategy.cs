namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class BuyLimitOrderStrategy : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMarketDataRepository _marketDataRepository;

        public BuyLimitOrderStrategy(
            IOrderRepository orderRepository,
            IMarketDataRepository marketDataRepository)
        {
            _orderRepository = orderRepository;
            _marketDataRepository = marketDataRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.BUY && request.Type == OrderType.LIMIT;
        }

        public async Task<OrderResultDto> ExecuteAsync(OrderRequestDto request)
        {
            // Obtener el cash disponible del usuario
            var availableCash = await _orderRepository.GetAvailableCashAsync(request.UserId);
            var totalCost = request.Size * request.Price;

            if (availableCash < totalCost)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient funds to buy the requested shares"
                };
            }

            // Obtener datos de mercado del instrumento
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

            // Si todas las condiciones son correctas, se crea la orden con estado 'NEW'
            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Price = (decimal)request.Price,
                Size = request.Size,
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
