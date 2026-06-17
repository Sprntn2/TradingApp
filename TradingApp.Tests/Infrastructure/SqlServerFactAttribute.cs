namespace TradingApp.Tests.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (!SqlServerTestHelper.IsAvailable())
        {
            Skip = "SQL Server is not available on this machine.";
        }
    }
}
