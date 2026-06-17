using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TradingApp.BusinessLayer.Services
{
    public class TradingSimulator : BackgroundService
    {
        private const int UpdateIntervalSeconds = 2;
        private const decimal MaxFluctuationPercent = 0.01m;

        private readonly ITradingCache _tradingCache;
        private readonly ILogger<TradingSimulator> _logger;
        private readonly Random _random = new();

        public TradingSimulator(
            ITradingCache tradingCache,
            ILogger<TradingSimulator> logger)
        {
            _tradingCache = tradingCache;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trading simulator started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updatedCount = await UpdateCachePricesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Updated {UpdatedCount} currency pairs in memory this cycle.",
                        updatedCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while simulating currency pair prices.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(UpdateIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            _logger.LogInformation("Trading simulator stopped.");
        }

        private async Task<int> UpdateCachePricesAsync(CancellationToken cancellationToken)
        {
            var updatedCount = 0;
            var pairs = _tradingCache.GetAllPairs();

            foreach (var pair in pairs)
            {
                var newPrice = CalculateFluctuatedPrice(pair.CurrentValue);

                if (newPrice <= 0)
                {
                    _logger.LogWarning(
                        "Skipped price update for pair {PairId} ({DisplayName}): calculated price was non-positive.",
                        pair.Id,
                        pair.DisplayName);
                    continue;
                }

                var result = await _tradingCache.TryUpdatePriceAsync(pair.Id, newPrice, cancellationToken);

                if (result is not null)
                {
                    updatedCount++;

                    _logger.LogDebug(
                        "Updated {DisplayName} in memory: {PreviousPrice} -> {NewPrice}",
                        pair.DisplayName,
                        result.PreviousPrice,
                        result.NewPrice);
                }
            }

            return updatedCount;
        }

        private decimal CalculateFluctuatedPrice(decimal currentPrice)
        {
            var fluctuationFactor = (((decimal)_random.NextDouble() * 2m) - 1m) * MaxFluctuationPercent;
            var newPrice = currentPrice * (1m + fluctuationFactor);

            return Math.Round(newPrice, 4);
        }
    }
}
