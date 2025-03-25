namespace CocosTradingAPI.Domain.Models
{
    public class Position
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int InstrumentId { get; set; }
        public int Quantity { get; set; }
        public decimal AvgPrice { get; set; }
    }
}
