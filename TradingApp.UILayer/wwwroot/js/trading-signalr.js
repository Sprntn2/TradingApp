const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/trading")
    .withAutomaticReconnect()
    .build();

connection.on("ReceivePriceUpdate", (data) => {
    TradingUI.updatePair(data.pairId, data);
});

connection.start().catch((error) => {
    console.error("SignalR connection failed:", error);
});
