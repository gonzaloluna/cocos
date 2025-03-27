namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using System;
    using System.Threading.Tasks;

    public class BuyLimitOrderStrategy : IOrderExecutionStrategy
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IInstrumentRepository _instrumentRepository;

        public BuyLimitOrderStrategy(
            IOrderRepository orderRepository,
            IInstrumentRepository instrumentRepository)
        {
            _orderRepository = orderRepository;
            _instrumentRepository = instrumentRepository;
        }

        public bool AppliesTo(OrderRequestDto request)
        {
            return request.Side == OrderSide.BUY && request.Type == OrderType.LIMIT;
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

            var cashAvailable = await _orderRepository.GetAvailableCashAsync(request.UserId);
            var totalCost = request.Price.Value * request.Size;

            if (cashAvailable < totalCost)
            {
                return new OrderResultDto
                {
                    Success = false,
                    Status = OrderStatus.REJECTED,
                    Message = "Insufficient funds for limit order"
                };
            }

            var order = new Order
            {
                UserId = request.UserId,
                InstrumentId = request.InstrumentId,
                Side = OrderSide.BUY,
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
                Message = "Limit buy order created"
            };
        }
    }
}
