using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TradingApp.BusinessLayer.Services;
using TradingApp.DataLayer;
using TradingApp.DataLayer.Models;
using TradingApp.Tests.Infrastructure;

namespace TradingApp.Tests.BusinessLayer;

public class TradingCacheBatchSyncTests
{
    [SqlServerFact]
    public async Task SyncCurrentValuesToDatabaseAsync_ExecutesSingleBatchUpdateCommand()
    {
        var interceptor = new SqlCommandCountingInterceptor();
        var databaseName = $"TradingBatchSyncTest_{Guid.NewGuid():N}";
        var connectionString = SqlServerTestHelper.CreateDatabaseConnectionString(databaseName);

        var services = new ServiceCollection();
        services.AddDbContext<TradingDbContext>(options =>
            options.UseSqlServer(connectionString).AddInterceptors(interceptor));
        services.AddLogging();

        await using var provider = services.BuildServiceProvider();
        await using var setupScope = provider.CreateAsyncScope();
        var setupContext = setupScope.ServiceProvider.GetRequiredService<TradingDbContext>();
        await setupContext.Database.MigrateAsync();

        var usd = new Currency { Country = "United States", Name = "Dollar", Abbreviation = "USD" };
        var ils = new Currency { Country = "Israel", Name = "Shekel", Abbreviation = "ILS" };
        var eur = new Currency { Country = "Europe", Name = "Euro", Abbreviation = "EUR" };
        setupContext.Currencies.AddRange(usd, ils, eur);
        await setupContext.SaveChangesAsync();

        setupContext.CurrencyPairs.AddRange(
            CurrencyPair.CreateForSeeding(usd.Id, ils.Id, 3.6500m, 3.6000m, 3.7000m),
            CurrencyPair.CreateForSeeding(eur.Id, usd.Id, 1.0800m, 1.0500m, 1.1200m));
        await setupContext.SaveChangesAsync();

        var pairs = setupContext.CurrencyPairs
            .Include(p => p.BaseCurrency)
            .Include(p => p.QuoteCurrency)
            .AsNoTracking()
            .ToList();

        var notifierMock = new Mock<IPriceUpdateNotifier>();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var logger = provider.GetRequiredService<ILogger<TradingCache>>();
        var cache = new TradingCache(scopeFactory, notifierMock.Object, logger);

        var pairsField = typeof(TradingCache).GetField(
            "_pairs",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var cachePairs = (ConcurrentDictionary<int, CurrencyPair>)pairsField!.GetValue(cache)!;

        foreach (var pair in pairs)
        {
            pair.UpdatePrice(pair.CurrentValue + 0.0100m);
            cachePairs[pair.Id] = pair;
        }

        var syncMethod = typeof(TradingCache).GetMethod(
            "SyncCurrentValuesToDatabaseAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(syncMethod);

        await (Task)syncMethod!.Invoke(cache, [CancellationToken.None])!;

        Assert.Equal(1, interceptor.NonQueryExecutionCount);

        await setupContext.Database.EnsureDeletedAsync();
    }
}
