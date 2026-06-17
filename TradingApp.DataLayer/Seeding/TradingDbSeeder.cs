using Microsoft.EntityFrameworkCore;
using TradingApp.DataLayer.Models;

namespace TradingApp.DataLayer.Seeding
{
    public static class TradingDbSeeder
    {
        public static async Task SeedAsync(
            TradingDbContext context,
            CancellationToken cancellationToken = default)
        {
            if (await context.Currencies.AnyAsync(cancellationToken))
            {
                return;
            }

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

            var eur = new Currency
            {
                Country = "Europe",
                Name = "Euro",
                Abbreviation = "EUR"
            };

            var gbp = new Currency
            {
                Country = "Great Britain",
                Name = "Pound",
                Abbreviation = "GBP"
            };

            context.Currencies.AddRange(usd, ils, eur, gbp);
            await context.SaveChangesAsync(cancellationToken);

            context.CurrencyPairs.AddRange(
                CurrencyPair.CreateForSeeding(usd.Id, ils.Id, 3.6500m, 3.6000m, 3.7000m),
                CurrencyPair.CreateForSeeding(eur.Id, usd.Id, 1.0800m, 1.0500m, 1.1200m),
                CurrencyPair.CreateForSeeding(gbp.Id, ils.Id, 4.6200m, 4.5500m, 4.7000m));

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
