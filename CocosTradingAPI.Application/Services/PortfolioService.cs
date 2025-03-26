using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Enums;
using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Application.Services
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IMarketDataRepository _marketDataRepo;

        public PortfolioService(IOrderRepository orderRepo, IMarketDataRepository marketDataRepo)
        {
            _orderRepo = orderRepo;
            _marketDataRepo = marketDataRepo;
        }

        public async Task<PortfolioDto> GetPortfolioAsync(int userId)
        {
            var orders = await _orderRepo.GetFilledOrdersByUserAsync(userId);

            var positions = new Dictionary<int, AssetPositionDto>();
            decimal availableCash = 0;

            foreach (var order in orders)
            {
                if (order.Instrument.Type == InstrumentType.MONEDA)
                {
                    if (order.Side == OrderSide.CASH_IN) availableCash += order.Size;
                    else if (order.Side == OrderSide.CASH_OUT) availableCash -= order.Size;
                }
                else
                {
                    if (!positions.ContainsKey(order.InstrumentId))
                    {
                        positions[order.InstrumentId] = new AssetPositionDto
                        {
                            Ticker = order.Instrument.Ticker,
                            Name = order.Instrument.Name,
                            Quantity = 0,
                            TotalValue = 0
                        };
                    }

                    var pos = positions[order.InstrumentId];

                    if (order.Side == OrderSide.BUY)
                    {
                        pos.Quantity += order.Size;
                        pos.TotalValue += order.Size * order.Price;
                    }
                    else if (order.Side == OrderSide.SELL)
                    {
                        pos.Quantity -= order.Size;
                        pos.TotalValue -= order.Size * order.Price;
                    }
                }
            }

            foreach (var kv in positions.ToList())
            {
                var pos = kv.Value;
                var market = await _marketDataRepo.GetLatestForInstrumentAsync(kv.Key);

                if (market != null && pos.Quantity > 0)
                {
                    var marketValue = pos.Quantity * market.Close;
                    var cost = pos.TotalValue;

                    pos.TotalValue = marketValue;
                    pos.ReturnPercentage = cost > 0 ? ((marketValue - cost) / cost) * 100 : 0;
                }
                else //TODO: a definir que sucede si no hay datos de mercado para un instrumento. Simplemente no lo listamos
                {
                    positions.Remove(kv.Key); 
                }
            }

            return new PortfolioDto
            {
                AvailableCash = availableCash,
                Positions = positions.Values.ToList(),
                TotalValue = availableCash + positions.Values.Sum(p => p.TotalValue)
            };
        }
    }
}
