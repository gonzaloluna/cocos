namespace CocosTradingAPI.Application.DTOs
{
    public class PortfolioResponse
    {
        public decimal TotalValue { get; set; }
        public decimal AvailableCash { get; set; }
        public List<PositionDto> Positions { get; set; } = new List<PositionDto>();
    }
}
