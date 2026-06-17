using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TradingApp.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyPairs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseCurrencyId = table.Column<int>(type: "int", nullable: false),
                    QuoteCurrencyId = table.Column<int>(type: "int", nullable: false),
                    CurrentValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyPairs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurrencyPairs_Currencies_BaseCurrencyId",
                        column: x => x.BaseCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurrencyPairs_Currencies_QuoteCurrencyId",
                        column: x => x.QuoteCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Abbreviation",
                table: "Currencies",
                column: "Abbreviation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyPairs_BaseCurrencyId_QuoteCurrencyId",
                table: "CurrencyPairs",
                columns: new[] { "BaseCurrencyId", "QuoteCurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyPairs_QuoteCurrencyId",
                table: "CurrencyPairs",
                column: "QuoteCurrencyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrencyPairs");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
