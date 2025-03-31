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
    public class SellMarketOrderStrategyTests
    {
        /// <summary>
        /// Verifica que SellMarketOrderStrategy ejecute correctamente una orden de venta de tipo MARKET
        /// cuando el usuario tiene suficientes acciones para vender y el precio de mercado es válido.
        /// En este escenario, el usuario tiene 10 acciones disponibles y el precio de mercado es 20,
        /// por lo que la orden se ejecuta correctamente (Success = true, Status = FILLED).
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldFillOrder_WhenUserHasSufficientSharesAndMarketPriceIsValid()
        {


            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.MARKET,
                Size = 5 // El usuario quiere vender 5 acciones
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellMarketOrderStrategy>>();

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order>
                         {
                     new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
                         }); // Usuario tiene 10 acciones

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 20m // Precio de mercado
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellMarketOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object
            );

            // Act
            var result = await strategy.ExecuteAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(OrderStatus.FILLED, result.Status);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.Is<Order>(o => o.UserId == request.UserId &&
                                                                        o.InstrumentId == request.InstrumentId &&
                                                                        o.Size == request.Size &&
                                                                        o.Price == 20m &&
                                                                        o.Status == OrderStatus.FILLED)), Times.Once);
        }

        /// <summary>
        /// Verifica que SellMarketOrderStrategy rechace una orden de venta de tipo MARKET
        /// cuando no hay datos de mercado disponibles para el instrumento seleccionado.
        /// En este caso, el market data es null, por lo que la orden debe ser rechazada 
        /// (Success = false, Status = REJECTED).
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
                Type = OrderType.MARKET,
                Size = 5
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellMarketOrderStrategy>>();

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                        .ReturnsAsync(new List<Order>
                        {
                            new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
                        }); // Usuario tiene 10 acciones

            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                        .ReturnsAsync(new List<MarketData>()); // No hay datos de mercado

            var strategy = new SellMarketOrderStrategy(
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
        /// Verifica que SellMarketOrderStrategy rechace una orden de venta de tipo MARKET
        /// cuando el usuario no tiene suficientes acciones disponibles para vender.
        /// En este caso, el usuario tiene 10 acciones, pero quiere vender 15, por lo que la orden debe ser rechazada 
        /// (Success = false, Status = REJECTED).
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
                Type = OrderType.MARKET,
                Size = 15 // El usuario tiene 10 acciones, pero quiere vender 15
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellMarketOrderStrategy>>();

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order>
                         {
                     new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
                         }); // Usuario tiene 10 acciones

            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 20m // Precio de mercado
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellMarketOrderStrategy(
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
        /// Verifica que SellMarketOrderStrategy rechace una orden de venta de tipo MARKET
        /// cuando el precio de mercado es 0 o negativo.
        /// En este caso, el precio de mercado es negativo, por lo que la orden debe ser rechazada 
        /// (Success = false, Status = REJECTED).
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenMarketPriceIsZeroOrNegative()
        {


            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.SELL,
                Type = OrderType.MARKET,
                Size = 5
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<SellMarketOrderStrategy>>();

            mockOrderRepo.Setup(repo => repo.GetFilledOrdersByUserAndInstrumentAsync(request.UserId, request.InstrumentId))
                         .ReturnsAsync(new List<Order>
                         {
                     new Order { Side = OrderSide.BUY, Size = 10, InstrumentId = request.InstrumentId }
                         }); // Usuario tiene 10 acciones

            // Simulamos un precio de mercado negativo
            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = -10m // Precio negativo
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            var strategy = new SellMarketOrderStrategy(
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
