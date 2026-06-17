using Moq;
using TradingApp.BusinessLayer.Mapping;
using TradingApp.DataLayer;
using TradingApp.DataLayer.Models;

namespace TradingApp.Tests.BusinessLayer;

public class CurrencyPairDtoMapperTests
{
    [Fact]
    public void ToDto_MapsAllFieldsFromLoadedPair()
    {
        var pair = CurrencyPair.CreateForSeeding(1, 2, 3.6500m, 3.6000m, 3.7000m);
        SetNavigationProperty(pair, "BaseCurrency", new Currency { Abbreviation = "USD" });
        SetNavigationProperty(pair, "QuoteCurrency", new Currency { Abbreviation = "ILS" });
        SetProperty(pair, nameof(CurrencyPair.Id), 42);

        var dto = CurrencyPairDtoMapper.ToDto(pair);

        Assert.Equal(42, dto.Id);
        Assert.Equal("USD / ILS", dto.DisplayName);
        Assert.Equal("USD", dto.BaseCurrencyCode);
        Assert.Equal("ILS", dto.QuoteCurrencyCode);
        Assert.Equal(3.6500m, dto.CurrentValue);
        Assert.Equal(3.6000m, dto.MinValue);
        Assert.Equal(3.7000m, dto.MaxValue);
    }

    private static void SetNavigationProperty(CurrencyPair pair, string propertyName, Currency currency)
    {
        typeof(CurrencyPair).GetProperty(propertyName)!.SetValue(pair, currency);
    }

    private static void SetProperty(CurrencyPair pair, string propertyName, int value)
    {
        typeof(CurrencyPair).GetProperty(propertyName)!.SetValue(pair, value);
    }
}
