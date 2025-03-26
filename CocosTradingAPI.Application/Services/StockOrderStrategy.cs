namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;
    using Microsoft.Extensions.Logging;
    public class StockOrderStrategy : IOrderProcessingStrategy
    {
        private readonly IDictionary<int, MarketData> _marketData;
        private readonly ILogger<StockOrderStrategy> _logger;
        public StockOrderStrategy(IDictionary<int, MarketData> marketData, ILogger<StockOrderStrategy> logger)
        {
            _marketData = marketData;
            _logger = logger;
        }

        public bool AppliesTo(Order order)
            => order.Instrument.Type == InstrumentType.ACCIONES;

        public void Process(Order order, PortfolioCalculationContext context)
        {
            if (!context.Positions.ContainsKey(order.InstrumentId))
            {
                context.Positions[order.InstrumentId] = new AssetPositionDto
                {
                    Ticker = order.Instrument.Ticker,
                    Name = order.Instrument.Name,
                    Quantity = 0,
                    Cost = 0,
                    TotalValue = 0,
                    ReturnPercentage = 0
                };
            }

            var position = context.Positions[order.InstrumentId];

            if (order.Side == OrderSide.BUY)
            {
                position.Quantity += order.Size;
                position.Cost += order.Size * order.Price;
            }
            else if (order.Side == OrderSide.SELL)
            {
                position.Quantity -= order.Size;
                // no se modifica el costo acumulado ya que tomo en cuenta solo lo que inverti
            }

            // aolo si hay acciones se calcula valor de mercado y retorno - si quantity diera negataivo habria una inconsistencia de datos?
            if (position.Quantity > 0 && _marketData.TryGetValue(order.InstrumentId, out var market))
            {
                var marketValue = position.Quantity * market.Close;
                position.TotalValue = marketValue;
                position.ReturnPercentage = position.Cost > 0
                    ? ((marketValue - position.Cost) / position.Cost) * 100
                    : 0;
            }
            else
            {
                //notificar a algun servicio de monitoreo por incongruencias?
                _logger.LogError("Position quantity is negative or market data is not available for instrument {InstrumentId}", order.InstrumentId);
                _logger.LogError("Position quantity {Quantity}", position.Quantity);
            }
        }
    }
}