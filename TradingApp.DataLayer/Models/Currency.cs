namespace TradingApp.DataLayer.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
    }
}
