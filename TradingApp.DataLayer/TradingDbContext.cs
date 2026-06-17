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
        }
    }
}
