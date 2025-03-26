namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using CocosTradingAPI.Domain.Models;

    public class CashOrderStrategy : IOrderProcessingStrategy
    {
        public bool AppliesTo(Order order)
            => order.Instrument.Type == InstrumentType.MONEDA;

        public void Process(Order order, PortfolioCalculationContext context)
        {
            if (order.Side == OrderSide.CASH_IN)
                context.AvailableCash += order.Size;
            else if (order.Side == OrderSide.CASH_OUT)
                context.AvailableCash -= order.Size;
        }
    }
}