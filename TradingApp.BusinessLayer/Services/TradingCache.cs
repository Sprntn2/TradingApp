using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.BusinessLayer.DTOs;
using TradingApp.BusinessLayer.Mapping;
using TradingApp.BusinessLayer.Persistence;
using TradingApp.DataLayer;
using TradingApp.DataLayer.Models;

namespace TradingApp.BusinessLayer.Services
{
    public class TradingCache : ITradingCache, IHostedService
    {
        private const int SyncIntervalMinutes = 5;

        private readonly ConcurrentDictionary<int, CurrencyPair> _pairs = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPriceUpdateNotifier _priceUpdateNotifier;
        private readonly ILogger<TradingCache> _logger;

        private CancellationTokenSource? _syncCancellation;
        private PeriodicTimer? _syncTimer;
        private Task? _syncBackgroundTask;

        public TradingCache(
            IServiceScopeFactory scopeFactory,
            IPriceUpdateNotifier priceUpdateNotifier,
            ILogger<TradingCache> logger)
        {
            _scopeFactory = scopeFactory;
            _priceUpdateNotifier = priceUpdateNotifier;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();

            var pairs = await context.CurrencyPairs
                .Include(p => p.BaseCurrency)
                .Include(p => p.QuoteCurrency)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            foreach (var pair in pairs)
            {
                _pairs[pair.Id] = pair;
            }

            _logger.LogInformation(
                "Trading cache initialized with {PairCount} currency pairs.",
                _pairs.Count);

            _syncCancellation = new CancellationTokenSource();
            _syncTimer = new PeriodicTimer(TimeSpan.FromMinutes(SyncIntervalMinutes));
            _syncBackgroundTask = RunPeriodicSyncAsync(_syncCancellation.Token);

            _logger.LogInformation(
                "Background database sync started (interval: {SyncIntervalMinutes} minutes).",
                SyncIntervalMinutes);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_syncCancellation is null)
            {
                return;
            }

            await _syncCancellation.CancelAsync();

            _syncTimer?.Dispose();

            if (_syncBackgroundTask is not null)
            {
                try
                {
                    await _syncBackgroundTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when the periodic sync loop is cancelled.
                }
            }

            await SyncCurrentValuesToDatabaseAsync(cancellationToken);

            _syncCancellation.Dispose();
            _syncCancellation = null;
            _syncTimer = null;
            _syncBackgroundTask = null;

            _logger.LogInformation("Trading cache stopped and final database sync completed.");
        }

        public IReadOnlyCollection<CurrencyPair> GetAllPairs()
        {
            return _pairs.Values.ToList();
        }

        public IReadOnlyList<CurrencyPairDto> GetPairsForView()
        {
            var pairsForView = new List<CurrencyPairDto>(_pairs.Count);

            foreach (var pair in _pairs.Values)
            {
                lock (pair)
                {
                    pairsForView.Add(CurrencyPairDtoMapper.ToDto(pair));
                }
            }

            return pairsForView
                .OrderBy(pair => pair.DisplayName, StringComparer.Ordinal)
                .ToList();
        }

        public async Task<PriceUpdateResult?> TryUpdatePriceAsync(
            int pairId,
            decimal newPrice,
            CancellationToken cancellationToken = default)
        {
            if (!_pairs.TryGetValue(pairId, out var pair))
            {
                return null;
            }

            ExtremesSnapshot? extremesToPersist = null;
            decimal previousPrice;
            int updatedPairId;
            decimal currentValue;
            decimal minValue;
            decimal maxValue;

            lock (pair)
            {
                previousPrice = pair.CurrentValue;
                var previousMin = pair.MinValue;
                var previousMax = pair.MaxValue;

                pair.UpdatePrice(newPrice);

                updatedPairId = pair.Id;
                currentValue = pair.CurrentValue;
                minValue = pair.MinValue;
                maxValue = pair.MaxValue;

                if (pair.MinValue != previousMin || pair.MaxValue != previousMax)
                {
                    extremesToPersist = new ExtremesSnapshot(pair.Id, pair.MinValue, pair.MaxValue);
                }
            }

            if (extremesToPersist is not null)
            {
                await WriteThroughExtremesAsync(extremesToPersist, cancellationToken);
            }

            BroadcastPriceUpdateNonBlocking(updatedPairId, currentValue, minValue, maxValue);

            return new PriceUpdateResult(previousPrice, newPrice);
        }

        private void BroadcastPriceUpdateNonBlocking(
            int pairId,
            decimal currentValue,
            decimal minValue,
            decimal maxValue)
        {
            _ = BroadcastPriceUpdateAsync(pairId, currentValue, minValue, maxValue);
        }

        private async Task BroadcastPriceUpdateAsync(
            int pairId,
            decimal currentValue,
            decimal minValue,
            decimal maxValue)
        {
            try
            {
                await _priceUpdateNotifier.BroadcastPriceUpdateAsync(
                    pairId,
                    currentValue,
                    minValue,
                    maxValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to broadcast SignalR price update for pair {PairId}.",
                    pairId);
            }
        }

        private async Task WriteThroughExtremesAsync(
            ExtremesSnapshot extremes,
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();

                var rowsAffected = await context.CurrencyPairs
                    .Where(p => p.Id == extremes.PairId)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(p => p.MinValue, extremes.MinValue)
                            .SetProperty(p => p.MaxValue, extremes.MaxValue),
                        cancellationToken);

                if (rowsAffected == 0)
                {
                    _logger.LogWarning(
                        "Write-through failed: currency pair {PairId} was not found in the database.",
                        extremes.PairId);
                    return;
                }

                _logger.LogInformation(
                    "Write-through persisted extremes for pair {PairId}: Min={MinValue}, Max={MaxValue}.",
                    extremes.PairId,
                    extremes.MinValue,
                    extremes.MaxValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Write-through persistence failed for currency pair {PairId}.",
                    extremes.PairId);
            }
        }

        private async Task SyncCurrentValuesToDatabaseAsync(CancellationToken cancellationToken)
        {
            var snapshots = new Dictionary<int, decimal>(_pairs.Count);

            foreach (var pair in _pairs.Values)
            {
                lock (pair)
                {
                    snapshots[pair.Id] = pair.CurrentValue;
                }
            }

            if (snapshots.Count == 0)
            {
                _logger.LogInformation("No currency pairs in cache to synchronize.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TradingDbContext>();

            var syncedCount = await BatchUpdateCurrentValuesAsync(context, snapshots, cancellationToken);

            _logger.LogInformation(
                "Write-back synchronized CurrentValue for {PairCount} currency pairs to database.",
                syncedCount);
        }

        private static Task<int> BatchUpdateCurrentValuesAsync(
            TradingDbContext context,
            IReadOnlyDictionary<int, decimal> snapshots,
            CancellationToken cancellationToken)
        {
            var command = CurrencyPairBatchUpdateSqlBuilder.Build(snapshots);
            return context.Database.ExecuteSqlRawAsync(command.Sql, command.Parameters, cancellationToken);
        }

        private async Task RunPeriodicSyncAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (await _syncTimer!.WaitForNextTickAsync(cancellationToken))
                {
                    try
                    {
                        await SyncCurrentValuesToDatabaseAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Periodic write-back synchronization failed.");
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Background database sync loop cancelled.");
            }
        }

        private sealed record ExtremesSnapshot(int PairId, decimal MinValue, decimal MaxValue);
    }
}
