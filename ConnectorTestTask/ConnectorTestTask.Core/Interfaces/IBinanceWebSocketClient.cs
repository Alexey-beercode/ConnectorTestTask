using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Interfaces
{
    public interface IBinanceWebSocketClient
    {
        event Action<Trade> NewTradeReceived;
        event Action<Candle> NewCandleReceived;

        Task ConnectAsync();
        Task SubscribeTrades(string pair);
        Task SubscribeCandles(string pair, int periodInSec);
        Task Unsubscribe(string pair, string interval = "1m");
        Task DisconnectAsync();
    }
}