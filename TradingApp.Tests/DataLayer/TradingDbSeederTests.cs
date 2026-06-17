using Microsoft.EntityFrameworkCore;
using TradingApp.DataLayer;
using TradingApp.DataLayer.Models;
using TradingApp.DataLayer.Seeding;

namespace TradingApp.Tests.DataLayer;

public class TradingDbSeederTests
{
    [Fact]
    public async Task SeedAsync_EmptyDatabase_InsertsCurrenciesAndPairs()
    {
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TradingDbContext(options);

        await TradingDbSeeder.SeedAsync(context);

        Assert.Equal(4, await context.Currencies.CountAsync());
        Assert.Equal(3, await context.CurrencyPairs.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_WhenCurrenciesExist_DoesNotInsertDuplicates()
    {
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new TradingDbContext(options);
        context.Currencies.Add(new Currency
        {
            Country = "United States",
            Name = "Dollar",
            Abbreviation = "USD"
        });
        await context.SaveChangesAsync();

        await TradingDbSeeder.SeedAsync(context);

        Assert.Equal(1, await context.Currencies.CountAsync());
        Assert.Equal(0, await context.CurrencyPairs.CountAsync());
    }
}
