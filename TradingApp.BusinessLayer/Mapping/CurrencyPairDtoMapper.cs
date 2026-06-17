using TradingApp.BusinessLayer.DTOs;
using TradingApp.DataLayer.Models;

namespace TradingApp.BusinessLayer.Mapping
{
    internal static class CurrencyPairDtoMapper
    {
        public static CurrencyPairDto ToDto(CurrencyPair pair)
        {
            return new CurrencyPairDto
            {
                Id = pair.Id,
                DisplayName = pair.DisplayName,
                BaseCurrencyCode = pair.BaseCurrencyCode,
                QuoteCurrencyCode = pair.QuoteCurrencyCode,
                CurrentValue = pair.CurrentValue,
                MinValue = pair.MinValue,
                MaxValue = pair.MaxValue
            };
        }
    }
}
