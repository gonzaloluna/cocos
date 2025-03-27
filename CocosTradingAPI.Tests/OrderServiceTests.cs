using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;

namespace CocosTradingAPI.Tests.Application.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<ILogger<OrderService>> _loggerMock = new();

        [Fact]
        public async Task PlaceOrderAsync_ShouldUseCorrectStrategyAndReturnResult()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 10,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 5
            };

            var expectedResult = new OrderResultDto
            {
                Success = true,
                Status = OrderStatus.FILLED,
                Message = "Success"
            };

            var mockStrategy = new Mock<IOrderExecutionStrategy>();
            mockStrategy.Setup(s => s.AppliesTo(It.IsAny<OrderRequestDto>())).Returns(true);
            mockStrategy.Setup(s => s.ExecuteAsync(It.IsAny<OrderRequestDto>())).ReturnsAsync(expectedResult);

            var service = new OrderService(new List<IOrderExecutionStrategy> { mockStrategy.Object }, _loggerMock.Object);

            // Act
            var result = await service.PlaceOrderAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(OrderStatus.FILLED, result.Status);
            Assert.Equal("Success", result.Message);
        }

        [Fact]
        public async Task PlaceOrderAsync_ShouldThrowException_WhenNoStrategyApplies()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 10,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 5
            };

            var mockStrategy = new Mock<IOrderExecutionStrategy>();
            mockStrategy.Setup(s => s.AppliesTo(It.IsAny<OrderRequestDto>())).Returns(false);

            var service = new OrderService(new List<IOrderExecutionStrategy> { mockStrategy.Object }, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(request));
        }
    }
}
