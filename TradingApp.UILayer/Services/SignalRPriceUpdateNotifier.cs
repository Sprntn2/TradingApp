using Microsoft.AspNetCore.SignalR;
using TradingApp.BusinessLayer.Services;
using TradingApp.UILayer.Hubs;

namespace TradingApp.UILayer.Services
{
    public class SignalRPriceUpdateNotifier : IPriceUpdateNotifier
    {
        private readonly IHubContext<TradingHub> _hubContext;

        public SignalRPriceUpdateNotifier(IHubContext<TradingHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task BroadcastPriceUpdateAsync(
            int pairId,
            decimal currentValue,
            decimal minValue,
            decimal maxValue,
            CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.All.SendAsync(
                "ReceivePriceUpdate",
                new { pairId, currentValue, minValue, maxValue },
                cancellationToken);
        }
    }
}
