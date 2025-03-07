using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ConnectorTestTask.Core.Helpers;
using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;
using Microsoft.Extensions.Logging;

namespace ConnectorTestTask.Core.Clients
{
   public class BinanceWebSocketClient : IBinanceWebSocketClient
{
    private ClientWebSocket _webSocket;
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<BinanceWebSocketClient> _logger;
    private const string BaseUri = "wss://stream.binance.com:9443/ws";
    private readonly ConcurrentDictionary<string, string> _subscriptions = new();

    public event Action<Trade> NewTradeReceived;
    public event Action<Candle> NewCandleReceived;

    public BinanceWebSocketClient(ILogger<BinanceWebSocketClient> logger)
    {
        _logger = logger;
    }

    public async Task ConnectAsync()
    {
        if (_webSocket?.State == WebSocketState.Open) return;
        _webSocket = new ClientWebSocket();
        await _webSocket.ConnectAsync(new Uri(BaseUri), _cts.Token);
        _ = StartReceiving();
    }

    public async Task SubscribeTrades(string pair)
    {
        await SubscribeToStream(pair, $"{pair.ToLower()}@trade");
    }

    public async Task SubscribeCandles(string pair, int periodInSec)
    {
        var interval = TimeConverter.ConvertToTimeframe(periodInSec);
        await SubscribeToStream(pair, $"{pair.ToLower()}@kline_{interval}");
    }

    public async Task Unsubscribe(string pair, string interval = "1m")
    {
        if (!_subscriptions.ContainsKey(pair)) return;
        string message = $"{{\"method\": \"UNSUBSCRIBE\", \"params\": [\"{pair.ToLower()}@trade\", \"{pair.ToLower()}@kline_{interval}\"], \"id\": 3}}";
        await SendMessageAsync(message);
        _subscriptions.TryRemove(pair, out _);
    }

    public async Task DisconnectAsync()
    {
        if (_webSocket?.State != WebSocketState.Open) return;
        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cts.Token);
        _webSocket.Dispose();
        _webSocket = null;
        _subscriptions.Clear();
    }

    private async Task SubscribeToStream(string pair, string streamName)
    {
        if (_subscriptions.ContainsKey(pair)) return;
        await ConnectAsync();
        string message = $"{{\"method\": \"SUBSCRIBE\", \"params\": [\"{streamName}\"], \"id\": 1}}";
        await SendMessageAsync(message);
        _subscriptions[pair] = streamName;
    }

    private async Task SendMessageAsync(string message)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, _cts.Token);
        }
    }

    private async Task StartReceiving()
    {
        var buffer = new byte[8192];
        while (_webSocket?.State == WebSocketState.Open)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
            ProcessMessage(Encoding.UTF8.GetString(buffer, 0, result.Count));
        }
    }

    private void ProcessMessage(string message)
    {
        var json = JsonSerializer.Deserialize<JsonElement>(message);
        if (!json.TryGetProperty("e", out var eventType)) return;
        json.TryGetProperty("s", out var symbolProperty);
        string pair = symbolProperty.GetString()?.ToUpper() ?? "UNKNOWN";

        if (eventType.GetString() == "trade")
        {
            var trade = BinanceParser.ParseTrade(json, pair);
            if (trade != null) NewTradeReceived?.Invoke(trade);
        }
        else if (eventType.GetString() == "kline")
        {
            var candle = BinanceParser.ParseCandle(json, pair);
            if (candle != null) NewCandleReceived?.Invoke(candle);
        }
    }
}
}