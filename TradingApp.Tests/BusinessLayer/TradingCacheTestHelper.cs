using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TradingApp.BusinessLayer.Services;
using TradingApp.DataLayer;
using TradingApp.DataLayer.Models;

namespace TradingApp.Tests.BusinessLayer;

internal static class TradingCacheTestHelper
{
    public static async Task<TradingCacheTestContext> CreateInitializedCacheAsync(
        Action<TradingDbContext>? seed = null)
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using (var setupContext = new TradingDbContext(options))
        {
            if (seed is not null)
            {
                seed(setupContext);
            }
            else
            {
                SeedDefaultPair(setupContext);
            }

            await setupContext.SaveChangesAsync();
        }

        var notifierMock = new Mock<IPriceUpdateNotifier>();
        notifierMock
            .Setup(n => n.BroadcastPriceUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<decimal>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddDbContext<TradingDbContext>(builder => builder.UseInMemoryDatabase(dbName));
        services.AddLogging();

        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var logger = provider.GetRequiredService<ILogger<TradingCache>>();

        var cache = new TradingCache(scopeFactory, notifierMock.Object, logger);
        await cache.StartAsync(CancellationToken.None);

        return new TradingCacheTestContext(cache, notifierMock, provider);
    }

    private static void SeedDefaultPair(TradingDbContext context)
    {
        var usd = new Currency
        {
            Country = "United States",
            Name = "Dollar",
            Abbreviation = "USD"
        };

        var ils = new Currency
        {
            Country = "Israel",
            Name = "Shekel",
            Abbreviation = "ILS"
        };

        context.Currencies.AddRange(usd, ils);
        context.CurrencyPairs.Add(
            CurrencyPair.CreateForSeeding(usd.Id, ils.Id, 3.6500m, 3.6000m, 3.7000m));
    }

    internal sealed class TradingCacheTestContext : IAsyncDisposable
    {
        public TradingCacheTestContext(
            TradingCache cache,
            Mock<IPriceUpdateNotifier> notifier,
            ServiceProvider provider)
        {
            Cache = cache;
            Notifier = notifier;
            Provider = provider;
        }

        public TradingCache Cache { get; }
        public Mock<IPriceUpdateNotifier> Notifier { get; }
        public ServiceProvider Provider { get; }

        public async ValueTask DisposeAsync()
        {
            await StopCacheWithoutDatabaseSyncAsync(Cache);
            await Provider.DisposeAsync();
        }

        private static async Task StopCacheWithoutDatabaseSyncAsync(TradingCache cache)
        {
            var cacheType = typeof(TradingCache);
            var syncCancellationField = cacheType.GetField(
                "_syncCancellation",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var syncTimerField = cacheType.GetField(
                "_syncTimer",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var syncBackgroundTaskField = cacheType.GetField(
                "_syncBackgroundTask",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            var syncCancellation = syncCancellationField?.GetValue(cache) as CancellationTokenSource;
            if (syncCancellation is null)
            {
                return;
            }

            await syncCancellation.CancelAsync();

            (syncTimerField?.GetValue(cache) as PeriodicTimer)?.Dispose();

            if (syncBackgroundTaskField?.GetValue(cache) is Task syncBackgroundTask)
            {
                try
                {
                    await syncBackgroundTask;
                }
                catch (OperationCanceledException)
                {
                }
            }

            syncCancellation.Dispose();
        }
    }
}
