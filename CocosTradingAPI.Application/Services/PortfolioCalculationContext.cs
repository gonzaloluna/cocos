namespace CocosTradingAPI.Application.Services
{
    using CocosTradingAPI.Application.DTOs;

    public class PortfolioCalculationContext
    {
        public decimal AvailableCash { get; set; } = 0;
        public Dictionary<int, AssetPositionDto> Positions { get; set; } = new();
    }
}