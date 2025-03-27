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
    public class SellLimitOrderStrategyTests
    {
        /// <summary>
        /// Verifica que SellLimitOrderStrategy cree correctamente una orden de venta de tipo LIMIT
        /// cuando el usuario tiene suficientes acciones y el precio de mercado es válido.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldCreateOrderAsNew_WhenUserHasSufficientSharesAndMarketPriceIsValid()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellLimitOrderStrategy>>();

            // Simulamos que el usuario tiene 10 acciones disponibles
            var existingOrders = new List<Order>
            {
                new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
            };

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(existingOrders);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 21m // Precio de mercado (mayor que el precio límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(OrderStatus.NEW, result.Status); // Verificamos que la orden se cree con estado NEW

            mockOrderRepo.Verify(repo => repo.AddAsync(It.Is<Order>(o => o.UserId == request.UserId &&
                                                                        o.InstrumentId == request.InstrumentId &&
                                                                        o.Size == request.Size &&
                                                                        o.Price == marketData.Close &&
                                                                        o.Status == OrderStatus.NEW)), Times.Once);
        }

        /// <summary>
        /// Verifica que SellLimitOrderStrategy rechace una orden de venta de tipo LIMIT
        /// cuando el usuario no tiene suficientes acciones disponibles para vender.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenUserHasInsufficientShares()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Size = 10,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();

            // Simulamos que el usuario tiene 5 acciones disponibles
            var existingOrders = new List<Order>
            {
                new Order { Side = OrderSide.BUY, Size = 5, InstrumentId = request.InstrumentId }
            };

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(existingOrders);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 21m // Precio de mercado (mayor que el precio límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status); // La orden debe ser rechazada

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }

        /// <summary>
        /// Verifica que SellLimitOrderStrategy rechace una orden de venta de tipo LIMIT
        /// cuando el precio de mercado es menor que el precio límite.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenMarketPriceIsLowerThanLimitPrice()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 25m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();

            // Simulamos que el usuario tiene 10 acciones disponibles
            var existingOrders = new List<Order>
            {
                new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
            };

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(existingOrders);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 20m // Precio de mercado (menor que el precio límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status); // La orden debe ser rechazada

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }

        /// <summary>
        /// Verifica que SellLimitOrderStrategy rechace una orden de venta de tipo LIMIT
        /// cuando no hay datos de mercado disponibles para el instrumento.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenNoMarketDataAvailable()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                Size = 5,
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();

            // Simulamos que el usuario tiene 10 acciones disponibles
            var existingOrders = new List<Order>
            {
                new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
            };

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(existingOrders);

            // No hay datos de mercado
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData>());

            var strategy = new SellLimitOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status); // La orden debe ser rechazada

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }
    }
}
