namespace CocosTradingAPI.Application.DTOs
{
    public class PositionDto
    {
        public string Ticker { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalValue { get; set; }
        public decimal DailyReturn { get; set; }
    }
}
