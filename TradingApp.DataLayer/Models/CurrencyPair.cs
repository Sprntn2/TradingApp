namespace TradingApp.DataLayer.Models
{
    public class CurrencyPair
    {
        public int Id { get; private set; }
        public int BaseCurrencyId { get; private set; }
        public int QuoteCurrencyId { get; private set; }
        public Currency? BaseCurrency { get; private set; }
        public Currency? QuoteCurrency { get; private set; }
        public decimal CurrentValue { get; private set; }
        public decimal MinValue { get; private set; }
        public decimal MaxValue { get; private set; }

        public string BaseCurrencyCode => BaseCurrency?.Abbreviation ?? string.Empty;
        public string QuoteCurrencyCode => QuoteCurrency?.Abbreviation ?? string.Empty;
        public string DisplayName => $"{BaseCurrencyCode} / {QuoteCurrencyCode}";

        private CurrencyPair()
        {
        }

        /// <summary>
        /// Updates the current price and expands <see cref="MinValue"/> / <see cref="MaxValue"/>
        /// when the new price exceeds the recorded range.
        /// </summary>
        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice <= 0)
            {
                throw new ArgumentException("Price must be greater than zero.", nameof(newPrice));
            }

            CurrentValue = newPrice;

            if (newPrice < MinValue)
            {
                MinValue = newPrice;
            }

            if (newPrice > MaxValue)
            {
                MaxValue = newPrice;
            }
        }
    }
}
