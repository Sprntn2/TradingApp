using System.Text;
using Microsoft.Data.SqlClient;

namespace TradingApp.BusinessLayer.Persistence
{
    internal static class CurrencyPairBatchUpdateSqlBuilder
    {
        internal sealed record BatchUpdateCommand(string Sql, IReadOnlyList<SqlParameter> Parameters);

        public static BatchUpdateCommand Build(IReadOnlyDictionary<int, decimal> snapshots)
        {
            var valueRows = new StringBuilder();
            var parameters = new List<SqlParameter>(snapshots.Count * 2);
            var index = 0;

            foreach (var (pairId, currentValue) in snapshots)
            {
                if (index > 0)
                {
                    valueRows.Append(", ");
                }

                valueRows.Append($"(@p{index}, @p{index + 1})");
                parameters.Add(new SqlParameter($"@p{index}", pairId));
                parameters.Add(new SqlParameter($"@p{index + 1}", currentValue) { Precision = 18, Scale = 4 });
                index += 2;
            }

            var sql = $"""
                UPDATE cp
                SET cp.CurrentValue = v.CurrentValue
                FROM CurrencyPairs cp
                INNER JOIN (VALUES {valueRows}) AS v(Id, CurrentValue) ON cp.Id = v.Id
                """;

            return new BatchUpdateCommand(sql, parameters);
        }
    }
}
