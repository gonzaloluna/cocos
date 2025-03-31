using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Enums;
using CocosTradingAPI.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace CocosTradingAPI.Tests.Application.Services
{
    public class OrderServiceTests
    {
        /// <summary>
        /// Verifica que el servicio de órdenes selecciona el strategy correcto 
        /// basado en las condiciones de `AppliesTo` del `OrderRequestDto` para cada tipo de orden.
        /// </summary>
        [Fact]
        public async Task PlaceOrderAsync_ShouldSelectCorrectStrategyBasedOnOrderType()
        {
            // Arrange
            var requestBuyMarket = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 5
            };

            var requestSellMarket = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.MARKET,
                Size = 5
            };

            var requestBuyLimit = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 50m
            };

            var requestSellLimit = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 50m
            };

            var requestBuyLimitWithTotalAmount = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                TotalAmount = 100m
            };

            var requestSellLimitWithTotalAmount = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                TotalAmount = 100m,
                Price = 50m
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<OrderService>>();

            // Mock de los strategies
            var mockBuyMarketStrategy = new Mock<IOrderExecutionStrategy>();
            var mockSellMarketStrategy = new Mock<IOrderExecutionStrategy>();
            var mockBuyLimitStrategy = new Mock<IOrderExecutionStrategy>();
            var mockSellLimitStrategy = new Mock<IOrderExecutionStrategy>();
            var mockBuyLimitWithTotalAmountStrategy = new Mock<IOrderExecutionStrategy>();
            var mockSellLimitWithTotalAmountStrategy = new Mock<IOrderExecutionStrategy>();

            // Setup de las condiciones para cada strategy
            mockBuyMarketStrategy.Setup(strategy => strategy.AppliesTo(requestBuyMarket)).Returns(true);
            mockSellMarketStrategy.Setup(strategy => strategy.AppliesTo(requestSellMarket)).Returns(true);
            mockBuyLimitStrategy.Setup(strategy => strategy.AppliesTo(requestBuyLimit)).Returns(true);
            mockSellLimitStrategy.Setup(strategy => strategy.AppliesTo(requestSellLimit)).Returns(true);
            mockBuyLimitWithTotalAmountStrategy.Setup(strategy => strategy.AppliesTo(requestBuyLimitWithTotalAmount)).Returns(true);
            mockSellLimitWithTotalAmountStrategy.Setup(strategy => strategy.AppliesTo(requestSellLimitWithTotalAmount)).Returns(true);

            // Mock de OrderService, inyectando las dependencias
            var orderService = new OrderService(
                new List<IOrderExecutionStrategy>
                {
                    mockBuyMarketStrategy.Object,
                    mockSellMarketStrategy.Object,
                    mockBuyLimitStrategy.Object,
                    mockSellLimitStrategy.Object,
                    mockBuyLimitWithTotalAmountStrategy.Object,
                    mockSellLimitWithTotalAmountStrategy.Object
                },
                mockLogger.Object
            );

            // Act: Ejecutar PlaceOrderAsync para los diferentes tipos de órdenes
            var resultBuyMarket = await orderService.PlaceOrderAsync(requestBuyMarket);
            var resultSellMarket = await orderService.PlaceOrderAsync(requestSellMarket);
            var resultBuyLimit = await orderService.PlaceOrderAsync(requestBuyLimit);
            var resultSellLimit = await orderService.PlaceOrderAsync(requestSellLimit);
            var resultBuyLimitWithTotalAmount = await orderService.PlaceOrderAsync(requestBuyLimitWithTotalAmount);
            var resultSellLimitWithTotalAmount = await orderService.PlaceOrderAsync(requestSellLimitWithTotalAmount);

            // Assert
            // Verificar que el BuyMarketOrderStrategy fue ejecutado
            mockBuyMarketStrategy.Verify(strategy => strategy.ExecuteAsync(It.IsAny<OrderRequestDto>()), Times.Once);

            // Verificar que el SellMarketOrderStrategy fue ejecutado
            mockSellMarketStrategy.Verify(strategy => strategy.ExecuteAsync(It.IsAny<OrderRequestDto>()), Times.Once);

            // Verificar que el BuyLimitOrderStrategy fue ejecutado
            mockBuyLimitStrategy.Verify(strategy => strategy.ExecuteAsync(It.IsAny<OrderRequestDto>()), Times.Once);

            // Verificar que el SellLimitOrderStrategy fue ejecutado
            mockSellLimitStrategy.Verify(strategy => strategy.ExecuteAsync(It.IsAny<OrderRequestDto>()), Times.Once);

            // Verificar que el BuyLimitOrderStrategyWithTotalAmount fue ejecutado
            mockBuyLimitWithTotalAmountStrategy.Verify(strategy => strategy.ExecuteAsync(It.IsAny<OrderRequestDto>()), Times.Once);

            // Verificar que el SellLimitOrderStrategyWithTotalAmount fue ejecutado
            mockSellLimitWithTotalAmountStrategy.Verify(strategy => strategy.ExecuteAsync(It.IsAny<OrderRequestDto>()), Times.Once);
        }
    }
}
