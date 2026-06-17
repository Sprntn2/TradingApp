using Microsoft.Data.SqlClient;

namespace TradingApp.Tests.Infrastructure;

internal static class SqlServerTestHelper
{
    internal const string DefaultConnectionString =
        "Server=localhost\\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

    internal static bool IsAvailable()
    {
        try
        {
            using var connection = new SqlConnection(DefaultConnectionString);
            connection.Open();
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static string CreateDatabaseConnectionString(string databaseName) =>
        $"Server=localhost\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";
}
