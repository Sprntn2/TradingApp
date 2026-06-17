using Microsoft.EntityFrameworkCore;
using TradingApp.DataLayer.Models;

namespace TradingApp.DataLayer
{
    public class TradingDbContext : DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<CurrencyPair> CurrencyPairs => Set<CurrencyPair>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("Currencies");
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Abbreviation).HasMaxLength(10);
                entity.HasIndex(e => e.Abbreviation).IsUnique();
            });

            modelBuilder.Entity<CurrencyPair>(entity =>
            {
                entity.ToTable("CurrencyPairs");
                entity.Property(e => e.CurrentValue).HasPrecision(18, 4);
                entity.Property(e => e.MinValue).HasPrecision(18, 4);
                entity.Property(e => e.MaxValue).HasPrecision(18, 4);

                entity.Ignore(e => e.BaseCurrencyCode);
                entity.Ignore(e => e.QuoteCurrencyCode);
                entity.Ignore(e => e.DisplayName);

                entity.HasOne(e => e.BaseCurrency)
                    .WithMany()
                    .HasForeignKey(e => e.BaseCurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.QuoteCurrency)
                    .WithMany()
                    .HasForeignKey(e => e.QuoteCurrencyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.BaseCurrencyId, e.QuoteCurrencyId }).IsUnique();
            });

            modelBuilder.Entity<Currency>().HasData(
                new { Id = 1, Country = "United States", Name = "Dollar", Abbreviation = "USD" },
                new { Id = 2, Country = "Israel", Name = "Shekel", Abbreviation = "ILS" },
                new { Id = 3, Country = "Europe", Name = "Euro", Abbreviation = "EUR" },
                new { Id = 4, Country = "Great Britain", Name = "Pound", Abbreviation = "GBP" });

            modelBuilder.Entity<CurrencyPair>().HasData(
                new { Id = 1, BaseCurrencyId = 1, QuoteCurrencyId = 2, CurrentValue = 3.6500m, MinValue = 3.6000m, MaxValue = 3.7000m },
                new { Id = 2, BaseCurrencyId = 3, QuoteCurrencyId = 1, CurrentValue = 1.0800m, MinValue = 1.0500m, MaxValue = 1.1200m },
                new { Id = 3, BaseCurrencyId = 4, QuoteCurrencyId = 2, CurrentValue = 4.6200m, MinValue = 4.5500m, MaxValue = 4.7000m });
        }
    }
}
