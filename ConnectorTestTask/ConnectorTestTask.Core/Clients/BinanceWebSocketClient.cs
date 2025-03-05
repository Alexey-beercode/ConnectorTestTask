using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using CancellationTokenSource = System.Threading.CancellationTokenSource;
using Uri = System.Uri;
using ConnectorTestTask.Core.Helpers;
using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Clients
{
    public class BinanceWebSocketClient : IBinanceWebSocketClient
    {
        private ClientWebSocket _webSocket;
        private readonly Uri _baseUri;
        private readonly CancellationTokenSource _cts = new();

        public event Action<Trade> NewTradeReceived;
        public event Action<Candle> NewCandleReceived;

        public BinanceWebSocketClient()
        {
            _baseUri = new Uri("wss://stream.binance.com:9443/ws/");
        }

        public async Task ConnectAsync()
        {
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(_baseUri, CancellationToken.None);
            await StartReceiving();
        }

        public async Task SubscribeTrades(string pair)
        {
            string message = $"{{\"method\": \"SUBSCRIBE\", \"params\": [\"{pair.ToLower()}@trade\"], \"id\": 1}}";
            await SendMessageAsync(message);
        }

        public async Task SubscribeCandles(string pair, int periodInSec)
        {
            string interval = TimeConverter.ConvertToTimeframe(periodInSec);
            string message =
                $"{{\"method\": \"SUBSCRIBE\", \"params\": [\"{pair.ToLower()}@kline_{interval}\"], \"id\": 2}}";
            await SendMessageAsync(message);
        }

        public async Task Unsubscribe(string pair, string interval = "1m")
        {
            string message =
                $"{{\"method\": \"UNSUBSCRIBE\", \"params\": [\"{pair.ToLower()}@trade\", \"{pair.ToLower()}@kline_{interval}\"], \"id\": 3}}";
            await SendMessageAsync(message);
        }

        public async Task DisconnectAsync()
        {
            _cts.Cancel();
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }

            _webSocket.Dispose();
        }

        private async Task SendMessageAsync(string message)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true,
                    CancellationToken.None);
            }
        }

        private async Task StartReceiving()
        {
            var buffer = new byte[8192];

            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessMessage(message);
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                if (message.Contains("\"e\":\"trade\""))
                {
                    ProcessTradeMessage(message);
                }
                else if (message.Contains("\"e\":\"kline\""))
                {
                    ProcessCandleMessage(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
            }
        }

        private void ProcessTradeMessage(string message)
        {
            var pair = ExtractPairFromMessage(message);
            if (string.IsNullOrEmpty(pair))
            {
                Console.WriteLine("Не удалось извлечь пару из сообщения о трейде.");
                return;
            }

            var trades = BinanceParser.ParseTrades(message, pair);
            foreach (var trade in trades)
            {
                if (trade != null)
                {
                    NewTradeReceived?.Invoke(trade);
                }
            }
        }

        private void ProcessCandleMessage(string message)
        {
            var pair = ExtractPairFromMessage(message);
            if (string.IsNullOrEmpty(pair))
            {
                Console.WriteLine("Не удалось извлечь пару из сообщения о свече.");
                return;
            }

            var candles = BinanceParser.ParseCandles(message, pair);
            foreach (var candle in candles)
            {
                if (candle != null)
                {
                    NewCandleReceived?.Invoke(candle);
                }
            }
        }

        private string ExtractPairFromMessage(string message)
        {
            try
            {
                var json = JsonSerializer.Deserialize<JsonElement>(message);
                if (json.TryGetProperty("s", out var symbolProperty))
                {
                    return symbolProperty.GetString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при извлечении пары: {ex.Message}");
            }
            return null;
        }
    }
}
