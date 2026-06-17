using TradingApp.BusinessLayer.DTOs;
using TradingApp.DataLayer.Models;

namespace TradingApp.BusinessLayer.Services
{
    public sealed record PriceUpdateResult(decimal PreviousPrice, decimal NewPrice);

    public interface ITradingCache
    {
        IReadOnlyCollection<CurrencyPair> GetAllPairs();

        IReadOnlyList<CurrencyPairDto> GetPairsForView();

        Task<PriceUpdateResult?> TryUpdatePriceAsync(
            int pairId,
            decimal newPrice,
            CancellationToken cancellationToken = default);
    }
}
