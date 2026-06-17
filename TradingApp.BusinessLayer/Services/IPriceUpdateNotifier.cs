namespace TradingApp.BusinessLayer.Services
{
    public interface IPriceUpdateNotifier
    {
        Task BroadcastPriceUpdateAsync(
            int pairId,
            decimal currentValue,
            decimal minValue,
            decimal maxValue,
            CancellationToken cancellationToken = default);
    }
}
