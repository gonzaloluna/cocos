namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Threading.Tasks;

    public class SellLimitOrderStrategy : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;

        public SellLimitOrderStrategy(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.SELL && request.Type == OrderType.LIMIT;
        }

        public async Task<OrderResultDto> ExecuteAsync(OrderRequestDto request)
        {
            if (!request.Price.HasValue || request.Price <= 0)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Price is required for LIMIT orders"
                };
            }

            var orders = await _orderRepository.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId);
            var stockQuantity = orders.Sum(o => o.Side == OrderSide.BUY ? o.Size : -o.Size);

            if (stockQuantity < request.Size)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient shares for limit order"
                };
            }

            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Price = request.Price.Value,
                Size = request.Size,
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