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
    public class SellLimitOrderStrategyWithTotalAmountTests
    {
        /// <summary>
        /// Verifica que SellLimitOrderStrategyWithTotalAmount ejecute correctamente una orden de venta de tipo LIMIT
        /// cuando el usuario tiene suficientes acciones y el precio de mercado está dentro del límite.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldFillOrder_WhenUserHasSufficientSharesAndMarketPriceIsWithinLimit()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                TotalAmount = 100m, // Total Amount
                Price = 20m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellLimitOrderStrategyWithTotalAmount>>();

            // Simulamos que el usuario tiene 10 acciones disponibles para vender
            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order> { new Order { Side = OrderSide.BUY, Size = 10 } });

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 21m // Precio de mercado (dentro del límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategyWithTotalAmount(
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
                                                                        o.Size == 4 && // El número de acciones que el usuario puede vender con el TotalAmount
                                                                        o.Price == 21m &&
                                                                        o.Status == OrderStatus.NEW)), Times.Once);
        }

        /// <summary>
        /// Verifica que SellLimitOrderStrategyWithTotalAmount rechace una orden de venta de tipo LIMIT
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
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                TotalAmount = 100m,
                Price = 20m
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellLimitOrderStrategyWithTotalAmount>>();

            // Simulamos que el usuario tiene 10 acciones disponibles para vender
            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order> { new Order { Side = OrderSide.BUY, Size = 10 } });

            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData>()); // No hay datos de mercado

            var strategy = new SellLimitOrderStrategyWithTotalAmount(
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
        /// Verifica que SellLimitOrderStrategyWithTotalAmount rechace una orden de venta de tipo LIMIT
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
                TotalAmount = 100m,
                Price = 50m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellLimitOrderStrategyWithTotalAmount>>();

            // Simulamos que el usuario tiene solo 1 accion disponible para vender
            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order> { new Order { Side = OrderSide.BUY, Size = 1 } });

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 50m // Precio de mercado (dentro del límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategyWithTotalAmount(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);
            Assert.Equal("Insufficient shares to sell", result.Message);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }

        /// <summary>
        /// Verifica que SellLimitOrderStrategyWithTotalAmount rechace una orden de venta de tipo LIMIT
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
                TotalAmount = 100m,
                Price = 50m // Límite de precio
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellLimitOrderStrategyWithTotalAmount>>();

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order> { new Order { Side = OrderSide.BUY, Size = 10 } });

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 40m // Precio de mercado (menor que el precio límite)
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategyWithTotalAmount(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);
            Assert.Equal("Market price is lower than the limit price", result.Message);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }

        /// <summary>
        /// Verifica que SellLimitOrderStrategyWithTotalAmount rechace una orden de venta de tipo LIMIT
        /// cuando el TotalAmount es menor o igual a 0.
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenTotalAmountIsZeroOrNegative()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.LIMIT,
                TotalAmount = 0m, // Monto total inválido
                Price = 50m
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellLimitOrderStrategyWithTotalAmount>>();

            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(150);

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 45m
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellLimitOrderStrategyWithTotalAmount(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);
            Assert.Equal("Total amount must be greater than zero", result.Message);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }
    }
}
