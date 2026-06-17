using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TradingApp.Tests.Infrastructure;

internal sealed class SqlCommandCountingInterceptor : DbCommandInterceptor
{
    public int NonQueryExecutionCount { get; private set; }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (command.CommandText.Contains("UPDATE cp", StringComparison.OrdinalIgnoreCase))
        {
            NonQueryExecutionCount++;
        }

        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }
}
