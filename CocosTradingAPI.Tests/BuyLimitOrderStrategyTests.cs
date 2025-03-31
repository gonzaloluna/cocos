using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using CocosTradingAPI.Application.DTOs;
using CocosTradingAPI.Application.Services;
using CocosTradingAPI.Application.Interfaces;
using CocosTradingAPI.Domain.Enums;
using CocosTradingAPI.Domain.Models;

namespace CocosTradingAPI.Tests.Application.Services
{
    public class BuyLimitOrderStrategyTests
    {
        /// <summary>
        /// Verifica que BuyLimitOrderStrategy ejecute correctamente una orden de compra de tipo LIMIT
        /// cuando el usuario tiene suficiente dinero y el precio del instrumento está dentro del límite.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldFillOrder_WhenUserHasSufficientFundsAndMarketPriceIsWithinLimit()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<BuyLimitOrderStrategy>>();

            // Simulamos que el usuario tiene 150 disponible (5 * 20 = 100 para la compra)
            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(150);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 19m // Precio de mercado
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new BuyLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(OrderStatus.NEW, result.Status);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.Is<Order>(o => o.UserId == request.UserId &&
                                                                        o.InstrumentId == request.InstrumentId &&
                                                                        o.Size == request.Size &&
                                                                        o.Price == request.Price &&
                                                                        o.Status == OrderStatus.NEW)), Times.Once);
        }

        /// <summary>
        /// Verifica que BuyLimitOrderStrategy rechace una orden de compra de tipo LIMIT
        /// cuando no hay datos de mercado disponibles para el instrumento seleccionado.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenNoMarketDataAvailable()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<BuyLimitOrderStrategy>>();

            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(150);

            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData>()); // No hay datos de mercado

            var strategy = new BuyLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);
            Assert.Equal("No market data available for the selected instrument", result.Message);
            
            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }

        /// <summary>
        /// Verifica que BuyLimitOrderStrategy rechace una orden de compra de tipo LIMIT
        /// cuando el precio de mercado es mayor que el precio límite.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenMarketPriceIsGreaterThanLimitPrice()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<BuyLimitOrderStrategy>>();

            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(150);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 25m // Precio de mercado (mayor que el precio límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new BuyLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }

        /// <summary>
        /// Verifica que BuyLimitOrderStrategy rechace una orden de compra de tipo LIMIT
        /// cuando el usuario no tiene suficiente dinero disponible para realizar la compra.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenUserHasInsufficientFunds()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<BuyLimitOrderStrategy>>();

            // Simulamos que el usuario tiene 50 disponible, lo que no es suficiente para comprar 5 acciones a 20 cada una
            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(50);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 19m // Precio de mercado
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new BuyLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }
    }
}
