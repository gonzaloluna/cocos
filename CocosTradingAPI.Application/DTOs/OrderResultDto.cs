using CocosTradingAPI.Domain.Enums;

namespace CocosTradingAPI.Application.DTOs
{
    public class OrderResultDto
    {
        public bool Success { get; set; }
        public OrderStatus  Status { get; set; } 
        public string Message { get; set; } = string.Empty;
    }
}