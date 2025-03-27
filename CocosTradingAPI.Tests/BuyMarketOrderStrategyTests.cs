using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Enums;
using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Tests.Application.Services
{
    public class BuyMarketOrderStrategyTests
    {
        /// <summary>
        /// Test: Orden de compra de mercado ejecutada correctamente.
        /// Reglas que debe cumplir:
        /// - El usuario tiene suficiente cash (mock de GetAvailableCashAsync)
        /// - El instrumento tiene un precio de mercado válido (Close > 0)
        /// - Se llama a AddAsync con una orden FILLED
        /// - Se devuelve OrderResultDto con Success = true y Status = FILLED
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldFillOrder_WhenCashIsSufficient()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 42,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 10
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            mockOrderRepo.Setup(r => r.GetAvailableCashAsync(request.UserId))
                .ReturnsAsync(1000); // usuario tiene 1000

            var marketData = new MarketData
            {
                InstrumentId = 42,
                Close = 50m
            };

            var mockMarketRepo = new Mock<IMarketDataRepository>();
            mockMarketRepo.Setup(r => r.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<MarketData> { marketData });

            var mockInstrumentRepo = new Mock<IInstrumentRepository>();

            var strategy = new BuyMarketOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object,
                mockInstrumentRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            mockOrderRepo.Verify(r => r.AddAsync(It.Is<Order>(
                o => o.UserId == request.UserId &&
                     o.InstrumentId == request.InstrumentId &&
                     o.Price == 50m &&
                     o.Size == 10 &&
                     o.Status == OrderStatus.FILLED
            )), Times.Once);

            Assert.True(result.Success);
            Assert.Equal(OrderStatus.FILLED, result.Status);
        }
    }
}