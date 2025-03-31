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
using Microsoft.Extensions.Logging;

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

            var mockLogger = new Mock<ILogger<BuyMarketOrderStrategy>>();
            var mockOrderServiceLogger = new Mock<ILogger<OrderService>>();
            
            var strategy = new BuyMarketOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object,
                mockLogger.Object
            );

            var orderService = new OrderService(new [] {strategy}, mockOrderServiceLogger.Object);
            // Act
            var result = await orderService.PlaceOrderAsync(request);
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

        /// <summary>
        /// Test: Orden de compra debe ser rechazada si no hay datos de mercado para el instrumento.
        /// - El repositorio devuelve lista vacía
        /// - No se debe llamar a AddAsync
        /// - Se devuelve OrderResultDto con Success = false y Status = REJECTED
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenNoMarketData()
        {
            // Arrange
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 99,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 5
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            mockOrderRepo.Setup(r => r.GetAvailableCashAsync(request.UserId))
                .ReturnsAsync(1000);

            var mockMarketRepo = new Mock<IMarketDataRepository>();
            mockMarketRepo.Setup(r => r.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<MarketData>()); // sin datos

            var mockLogger = new Mock<ILogger<BuyMarketOrderStrategy>>();
            var mockOrderServiceLogger = new Mock<ILogger<OrderService>>();
            
            var strategy = new BuyMarketOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object,
                mockLogger.Object
            );

            var orderService = new OrderService(new [] {strategy}, mockOrderServiceLogger.Object);
            // Act
            var result = await orderService.PlaceOrderAsync(request);

            // Assert
            mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
            Assert.False(result.Success);
            Assert.Equal(OrderStatus.REJECTED, result.Status);
            Assert.Equal("No market data available for the selected instrument", result.Message);

            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never); // No debe llamar AddAsync
        }


        /// <summary>
        /// Verifica que BuyMarketOrderStrategy rechace una orden de compra de tipo MARKET 
        /// cuando el usuario no tiene fondos suficientes.
        /// En este escenario, el usuario tiene 100 de efectivo disponible y el precio de mercado 
        /// es 20 para 10 unidades (requiere un total de 200), por lo que la orden debe ser rechazada 
        /// (Success = false, Status = REJECTED) y no se debe agregar ninguna orden al repositorio (AddAsync no es llamado).
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectMarketOrder_WhenUserHasInsufficientCash()
        {
            // Arrange: configurar el escenario del test
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 10
            };

            // Crear mocks para las dependencias
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<BuyMarketOrderStrategy>>();
            var mockOrderServiceLogger = new Mock<ILogger<OrderService>>();
            
            // Simular cash disponible del usuario = 100
            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(100);

            // Simular datos de mercado, precio = 20
            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 20m // Precio de 20
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            // Instanciar la estrategia con los mocks
            var strategy = new BuyMarketOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object,
                mockLogger.Object
            );

            var orderService = new OrderService(new [] {strategy}, mockOrderServiceLogger.Object);
            // Act
            var result = await orderService.PlaceOrderAsync(request);

            // Assert: verificar que el resultado sea rechazo y no se guarde la orden
            Assert.False(result.Success, "La operación debería fallar por fondos insuficientes");
            Assert.Equal(OrderStatus.REJECTED, result.Status);

            // Verificar que no se haya llamado a AddAsync en el repositorio de órdenes
            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never,
                "No se debe guardar la orden cuando el usuario no tiene fondos suficientes");
        }

        /// <summary>
        /// Verifica que BuyMarketOrderStrategy rechace una orden de compra de tipo MARKET 
        /// cuando el precio de mercado es 0 o negativo.
        /// En este escenario, el precio de mercado es 0, por lo que la orden debe ser rechazada 
        /// (Success = false, Status = REJECTED) y no se debe agregar ninguna orden al repositorio (AddAsync no es llamado).
        /// </summary>
        [Fact]
        public async Task ExecuteAsync_ShouldRejectOrder_WhenMarketPriceIsZeroOrNegative()
        {
            // Arrange: configurar el escenario del test
            var request = new OrderRequestDto
            {
                UserId = 1,
                InstrumentId = 1,
                Side = OrderSide.BUY,
                Type = OrderType.MARKET,
                Size = 10
            };

            // Crear mocks para las dependencias
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockMarketRepo = new Mock<IMarketDataRepository>();
            var mockLogger = new Mock<ILogger<BuyMarketOrderStrategy>>();
            var mockOrderServiceLogger = new Mock<ILogger<OrderService>>();
            
            // Simular cash disponible del usuario = 100
            mockOrderRepo.Setup(repo => repo.GetAvailableCashAsync(request.UserId))
                         .ReturnsAsync(100);

            // Simular precio de mercado = 0
            var marketData = new MarketData
            {
                InstrumentId = request.InstrumentId,
                Close = 0m // Precio 0
            };
            mockMarketRepo.Setup(repo => repo.GetLatestForInstrumentsAsync(It.IsAny<IEnumerable<int>>()))
                          .ReturnsAsync(new List<MarketData> { marketData });

            // Instanciar la estrategia con los mocks
            var strategy = new BuyMarketOrderStrategy(
                mockOrderRepo.Object,
                mockMarketRepo.Object,
                mockLogger.Object
            );

            var orderService = new OrderService(new [] {strategy}, mockOrderServiceLogger.Object);
            // Act
            var result = await orderService.PlaceOrderAsync(request);

            // Assert: verificar que el resultado sea rechazo y no se guarde la orden
            Assert.False(result.Success, "La operación debería fallar debido a precio de mercado 0 o negativo");
            Assert.Equal(OrderStatus.REJECTED, result.Status);

            // Verificar que no se haya llamado a AddAsync en el repositorio de órdenes
            mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never,
                "No se debe guardar la orden cuando el precio de mercado es 0 o negativo");
        }

    }
}