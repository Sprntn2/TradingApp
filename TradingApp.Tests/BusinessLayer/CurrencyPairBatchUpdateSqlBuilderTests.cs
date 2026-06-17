using TradingApp.BusinessLayer.Persistence;

namespace TradingApp.Tests.BusinessLayer;

public class CurrencyPairBatchUpdateSqlBuilderTests
{
    [Fact]
    public void Build_SinglePair_ProducesOneUpdateStatement()
    {
        var command = CurrencyPairBatchUpdateSqlBuilder.Build(
            new Dictionary<int, decimal> { [1] = 3.6500m });

        Assert.StartsWith("UPDATE cp", command.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ExecuteUpdate", command.Sql, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, command.Parameters.Count);
        Assert.Equal(1, command.Parameters[0].Value);
        Assert.Equal(3.6500m, command.Parameters[1].Value);
    }

    [Fact]
    public void Build_ThreePairs_ProducesSingleStatementWithThreeValueRows()
    {
        var snapshots = new Dictionary<int, decimal>
        {
            [1] = 3.6500m,
            [2] = 1.0800m,
            [3] = 4.6200m
        };

        var command = CurrencyPairBatchUpdateSqlBuilder.Build(snapshots);

        Assert.Equal(1, CountOccurrences(command.Sql, "UPDATE cp"));
        Assert.Equal(3, CountOccurrences(command.Sql, "(@p"));
        Assert.Equal(6, command.Parameters.Count);
        Assert.All(command.Parameters, parameter => Assert.StartsWith("@p", parameter.ParameterName));
    }

    private static int CountOccurrences(string source, string value) =>
        source.Split(value, StringSplitOptions.None).Length - 1;
}
