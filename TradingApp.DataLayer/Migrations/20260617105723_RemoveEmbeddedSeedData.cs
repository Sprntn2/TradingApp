using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TradingApp.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEmbeddedSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CurrencyPairs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CurrencyPairs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CurrencyPairs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Abbreviation", "Country", "Name" },
                values: new object[,]
                {
                    { 1, "USD", "United States", "Dollar" },
                    { 2, "ILS", "Israel", "Shekel" },
                    { 3, "EUR", "Europe", "Euro" },
                    { 4, "GBP", "Great Britain", "Pound" }
                });

            migrationBuilder.InsertData(
                table: "CurrencyPairs",
                columns: new[] { "Id", "BaseCurrencyId", "CurrentValue", "MaxValue", "MinValue", "QuoteCurrencyId" },
                values: new object[,]
                {
                    { 1, 1, 3.6500m, 3.7000m, 3.6000m, 2 },
                    { 2, 3, 1.0800m, 1.1200m, 1.0500m, 1 },
                    { 3, 4, 4.6200m, 4.7000m, 4.5500m, 2 }
                });
        }
    }
}
