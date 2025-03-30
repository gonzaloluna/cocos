namespace CocosTradingAPI.WebAPI.Controllers
{
    using CocosTradingAPI.Application.DTOs;
    using CocosTradingAPI.Application.Interfaces;
    using CocosTradingAPI.Domain.Enums;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderRequestDto request)
        {

            var validationResult = ValidateOrderRequest(request);
            if (validationResult != null)
            {
                return validationResult; 
            }
            
            var result = await _orderService.PlaceOrderAsync(request);
            if (result.Success)
            {
                return Ok(result);  
            }

            switch (result.Status)
            {
                case OrderStatus.REJECTED:
                    return BadRequest(result); 
                case OrderStatus.CANCELLED:
                    return Conflict(result); 
                default:
                    return StatusCode(500, result);
            }
        }

        private IActionResult ValidateOrderRequest(OrderRequestDto request)
        {
            // Validación: No se puede enviar `Size` y `TotalAmount` al mismo tiempo
            if (request.Size > 0 && request.TotalAmount > 0)
            {
                return BadRequest("You cannot specify both size and total amount in the same request.");
            }

            // Validación: Si se envía `Size`, debe ser mayor que 0
            if (request.Size <= 0 && request.TotalAmount <= 0)
            {
                return BadRequest("You must specify either size or total amount to place an order.");
            }

            // Validación: Si se envía `TotalAmount`, debe ser mayor que 0
            if (request.TotalAmount <= 0 && request.Size == 0)
            {
                return BadRequest("Total amount must be greater than zero.");
            }

            // Validación: `Price` debe ser mayor que 0 para órdenes LIMIT
            if (request.Type == OrderType.LIMIT && request.Price <= 0)
            {
                return BadRequest("Price must be greater than zero for LIMIT orders.");
            }
            return null; // no errors
        }
    }
}
