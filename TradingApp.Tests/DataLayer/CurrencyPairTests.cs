using TradingApp.DataLayer.Models;

namespace TradingApp.Tests.DataLayer;

public class CurrencyPairTests
{
    [Fact]
    public void UpdatePrice_WithinRange_UpdatesCurrentValueOnly()
    {
        var pair = CurrencyPair.CreateForSeeding(1, 2, 3.6500m, 3.6000m, 3.7000m);

        pair.UpdatePrice(3.6800m);

        Assert.Equal(3.6800m, pair.CurrentValue);
        Assert.Equal(3.6000m, pair.MinValue);
        Assert.Equal(3.7000m, pair.MaxValue);
    }

    [Fact]
    public void UpdatePrice_BelowMin_ExpandsMinValue()
    {
        var pair = CurrencyPair.CreateForSeeding(1, 2, 3.6500m, 3.6000m, 3.7000m);

        pair.UpdatePrice(3.5500m);

        Assert.Equal(3.5500m, pair.CurrentValue);
        Assert.Equal(3.5500m, pair.MinValue);
        Assert.Equal(3.7000m, pair.MaxValue);
    }

    [Fact]
    public void UpdatePrice_AboveMax_ExpandsMaxValue()
    {
        var pair = CurrencyPair.CreateForSeeding(1, 2, 3.6500m, 3.6000m, 3.7000m);

        pair.UpdatePrice(3.7500m);

        Assert.Equal(3.7500m, pair.CurrentValue);
        Assert.Equal(3.6000m, pair.MinValue);
        Assert.Equal(3.7500m, pair.MaxValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdatePrice_NonPositivePrice_ThrowsArgumentException(decimal invalidPrice)
    {
        var pair = CurrencyPair.CreateForSeeding(1, 2, 3.6500m, 3.6000m, 3.7000m);

        var exception = Assert.Throws<ArgumentException>(() => pair.UpdatePrice(invalidPrice));

        Assert.Equal("newPrice", exception.ParamName);
    }
}
