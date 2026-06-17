namespace TradingApp.BusinessLayer.DTOs
{
    public class CurrencyPairDto
    {
        public int Id { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string BaseCurrencyCode { get; init; } = string.Empty;
        public string QuoteCurrencyCode { get; init; } = string.Empty;
        public decimal CurrentValue { get; init; }
        public decimal MinValue { get; init; }
        public decimal MaxValue { get; init; }
    }
}
