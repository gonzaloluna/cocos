  namespace CocosTradingAPI.Application.DTOs
{
    public class AssetPositionDto
    {
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal ReturnPercentage { get; set; }
    }
}