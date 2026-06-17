using Microsoft.Extensions.Logging;
using Moq;
using TradingApp.BusinessLayer.Services;
using TradingApp.DataLayer.Models;

namespace TradingApp.Tests.BusinessLayer;

public class TradingSimulatorTests
{
    [Fact]
    public async Task ExecuteAsync_UpdatesCacheAtLeastOnceBeforeCancellation()
    {
        var pair = CurrencyPair.CreateForSeeding(1, 2, 3.6500m, 3.6000m, 3.7000m);
        typeof(CurrencyPair).GetProperty(nameof(CurrencyPair.Id))!.SetValue(pair, 1);

        var cacheMock = new Mock<ITradingCache>();
        cacheMock.Setup(c => c.GetAllPairs()).Returns(new[] { pair });
        cacheMock
            .Setup(c => c.TryUpdatePriceAsync(
                pair.Id,
                It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceUpdateResult(3.6500m, 3.6600m));

        var logger = new Mock<ILogger<TradingSimulator>>();
        var simulator = new TradingSimulator(cacheMock.Object, logger.Object);

        using var cts = new CancellationTokenSource();
        await simulator.StartAsync(cts.Token);

        await Task.Delay(100);
        cts.Cancel();

        await simulator.StopAsync(CancellationToken.None);

        cacheMock.Verify(
            c => c.TryUpdatePriceAsync(
                pair.Id,
                It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }
}
