namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class SellLimitOrderStrategy : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMarketDataRepository _marketDataRepository;

        public SellLimitOrderStrategy(
            IOrderRepository orderRepository,
            IMarketDataRepository marketDataRepository)
        {
            _orderRepository = orderRepository;
            _marketDataRepository = marketDataRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.SELL && request.Type == OrderType.LIMIT && request.TotalAmount ==null;
        }

        public async Task<OrderResultDto> ExecuteAsync(OrderRequestDto request)
        {
            // Obtener las acciones disponibles para la venta
            var orders = await _orderRepository.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId);
            var stockQuantity = orders.Sum(o => o.Side == OrderSide.BUY ? o.Size : -o.Size);

            if (stockQuantity < request.Size)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient shares to sell"
                };
            }

            // Obtener datos de mercado del instrumento
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

            var marketPrice = marketData.Close;

            // Crear la orden con estado NEW
            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Price = marketPrice,
                Size = request.Size,
                Status = OrderStatus.NEW, // El estado es NEW, no FILLED
                DateTime = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);

            return new OrderResultDto
            {
                Success = true,
                Status = OrderStatus.NEW, // El estado sigue siendo NEW al momento de la creación
                Message = "Limit sell order created"
            };
        }
    }
}
