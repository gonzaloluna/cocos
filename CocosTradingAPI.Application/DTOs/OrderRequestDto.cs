using CocosTradingAPI.Domain.Enums;

namespace CocosTradingAPI.Application.DTOs
{
    public class OrderRequestDto
    {
        public int UserId { get; set; }
        public int InstrumentId { get; set; }
        public OrderSide  Side { get; set; }
        public OrderType  Type { get; set; }
        public int Size { get; set; }
        public decimal? Price { get; set; }
    }
}