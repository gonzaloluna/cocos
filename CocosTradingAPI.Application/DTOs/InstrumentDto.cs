namespace CocosTradingAPI.Application.DTOs
{
    public class InstrumentDto
    {
        public int Id { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public decimal Close { get; set; }
        public decimal PreviousClose { get; set; }
        public decimal DailyReturn { get; set; } 
    }
}
