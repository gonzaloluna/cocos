namespace CocosTradingAPI.Application.DTOs
{
    public class PortfolioDto
    {
        public decimal TotalValue { get; set; }
        public decimal AvailableCash { get; set; }
        public List<AssetPositionDto> Positions { get; set; } = new();
    }

  
}
