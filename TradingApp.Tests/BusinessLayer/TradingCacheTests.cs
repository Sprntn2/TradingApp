using Moq;
using TradingApp.BusinessLayer.Services;
using TradingApp.DataLayer;
using TradingApp.DataLayer.Models;

namespace TradingApp.Tests.BusinessLayer;

public class TradingCacheTests
{
    [Fact]
    public async Task StartAsync_LoadsPairsFromDatabase()
    {
        await using var context = await TradingCacheTestHelper.CreateInitializedCacheAsync();

        var pairs = context.Cache.GetAllPairs();

        Assert.Single(pairs);
        Assert.Equal(3.6500m, pairs.First().CurrentValue);
    }

    [Fact]
    public async Task GetPairsForView_ReturnsPairsSortedByDisplayName()
    {
        await using var context = await TradingCacheTestHelper.CreateInitializedCacheAsync(seed =>
        {
            var eur = new Currency { Country = "Europe", Name = "Euro", Abbreviation = "EUR" };
            var gbp = new Currency { Country = "Great Britain", Name = "Pound", Abbreviation = "GBP" };
            var usd = new Currency { Country = "United States", Name = "Dollar", Abbreviation = "USD" };

            seed.Currencies.AddRange(eur, gbp, usd);
            seed.SaveChanges();

            seed.CurrencyPairs.AddRange(
                CurrencyPair.CreateForSeeding(gbp.Id, usd.Id, 1.2700m, 1.2500m, 1.2900m),
                CurrencyPair.CreateForSeeding(eur.Id, usd.Id, 1.0800m, 1.0500m, 1.1200m));
        });

        var pairsForView = context.Cache.GetPairsForView();

        Assert.Equal(2, pairsForView.Count);
        Assert.Equal("EUR / USD", pairsForView[0].DisplayName);
        Assert.Equal("GBP / USD", pairsForView[1].DisplayName);
    }

    [Fact]
    public async Task TryUpdatePriceAsync_UnknownPair_ReturnsNull()
    {
        await using var context = await TradingCacheTestHelper.CreateInitializedCacheAsync();

        var result = await context.Cache.TryUpdatePriceAsync(999, 1.0000m);

        Assert.Null(result);
        context.Notifier.Verify(
            n => n.BroadcastPriceUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TryUpdatePriceAsync_ValidPair_UpdatesCacheAndBroadcasts()
    {
        await using var context = await TradingCacheTestHelper.CreateInitializedCacheAsync();

        var pairId = context.Cache.GetAllPairs().Single().Id;
        var result = await context.Cache.TryUpdatePriceAsync(pairId, 3.6800m);

        Assert.NotNull(result);
        Assert.Equal(3.6500m, result!.PreviousPrice);
        Assert.Equal(3.6800m, result.NewPrice);
        Assert.Equal(3.6800m, context.Cache.GetAllPairs().Single().CurrentValue);

        context.Notifier.Verify(
            n => n.BroadcastPriceUpdateAsync(
                pairId,
                3.6800m,
                3.6000m,
                3.7000m,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TryUpdatePriceAsync_NewExtreme_UpdatesInMemoryExtremes()
    {
        await using var context = await TradingCacheTestHelper.CreateInitializedCacheAsync();

        var pairId = context.Cache.GetAllPairs().Single().Id;

        await context.Cache.TryUpdatePriceAsync(pairId, 3.7500m);

        var pair = context.Cache.GetAllPairs().Single();
        Assert.Equal(3.7500m, pair.CurrentValue);
        Assert.Equal(3.6000m, pair.MinValue);
        Assert.Equal(3.7500m, pair.MaxValue);
    }
}
