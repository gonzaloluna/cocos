namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using Microsoft.Extensions.Logging;
    public class OrderService : IOrderService
    {
        private readonly IEnumerable<IOrderExecutionStrategy> _strategies;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IEnumerable<IOrderExecutionStrategy> strategies, ILogger<OrderService> logger)
        {
            _strategies = strategies;
            _logger = logger;
        }

        public async Task<OrderResultDto> PlaceOrderAsync(OrderRequestDto request)
        {
            var strategy = _strategies.FirstOrDefault(s => s.AppliesTo(request));
            if (strategy == null)
            {
                _logger.LogError("No strategy found for request: {@Request}", request);
                throw new InvalidOperationException("No applicable strategy found for the given order request.");
            }

            return await strategy.ExecuteAsync(request);
        }
    }
}
