using Microsoft.AspNetCore.SignalR;
using Moq;
using TradingApp.UILayer.Hubs;
using TradingApp.UILayer.Services;

namespace TradingApp.Tests.UILayer;

public class SignalRPriceUpdateNotifierTests
{
    [Fact]
    public async Task BroadcastPriceUpdateAsync_SendsReceivePriceUpdateToAllClients()
    {
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock
            .Setup(proxy => proxy.SendCoreAsync(
                "ReceivePriceUpdate",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(clients => clients.All).Returns(clientProxyMock.Object);

        var hubContextMock = new Mock<IHubContext<TradingHub>>();
        hubContextMock.Setup(hub => hub.Clients).Returns(clientsMock.Object);

        var notifier = new SignalRPriceUpdateNotifier(hubContextMock.Object);

        await notifier.BroadcastPriceUpdateAsync(7, 3.6800m, 3.6000m, 3.7000m);

        clientProxyMock.Verify(
            proxy => proxy.SendCoreAsync(
                "ReceivePriceUpdate",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var capturedArgs = clientProxyMock.Invocations
            .Single(i => i.Method.Name == nameof(IClientProxy.SendCoreAsync))
            .Arguments[1] as object?[];

        Assert.NotNull(capturedArgs);
        Assert.Single(capturedArgs!);
        Assert.True(HasExpectedPayload(capturedArgs![0]!, 7, 3.6800m, 3.6000m, 3.7000m));
    }

    private static bool HasExpectedPayload(
        object payload,
        int pairId,
        decimal currentValue,
        decimal minValue,
        decimal maxValue)
    {
        var type = payload.GetType();
        return (int)type.GetProperty("pairId")!.GetValue(payload)! == pairId
            && (decimal)type.GetProperty("currentValue")!.GetValue(payload)! == currentValue
            && (decimal)type.GetProperty("minValue")!.GetValue(payload)! == minValue
            && (decimal)type.GetProperty("maxValue")!.GetValue(payload)! == maxValue;
    }
}
