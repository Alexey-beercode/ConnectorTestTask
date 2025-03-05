using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ConnectorTestTask.Core.Implementations;
using ConnectorTestTask.Core.Clients;
using ConnectorTestTask.Core.Models;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConnectorTestTask.Tests
{
    public class BinanceConnectorTests : IAsyncLifetime
    {
        private readonly BinanceConnector _connector;
        private readonly BinanceRestClient _restClient;
        private readonly BinanceWebSocketClient _webSocketClient;
        private readonly Mock<ILogger<BinanceConnector>> _loggerMock;

        public BinanceConnectorTests()
        {
            _restClient = new BinanceRestClient(new HttpClient { BaseAddress = new Uri("https://api.binance.com/api/v3/") });
            _webSocketClient = new BinanceWebSocketClient();
            _loggerMock = new Mock<ILogger<BinanceConnector>>();
            _connector = new BinanceConnector(_restClient, _webSocketClient);
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => Task.CompletedTask;

        [Fact]
        public async Task GetAvailableCurrenciesAsync_ShouldReturn_Currencies()
        {
            var result = await _connector.GetAvailableCurrenciesAsync();

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Contains("BTC", result);
            Assert.Contains("USDT", result);
        }

        [Fact]
        public async Task GetNewTradesAsync_ShouldReturn_Trades()
        {
            var trades = await _connector.GetNewTradesAsync("BTCUSDT", 5);

            Assert.NotNull(trades);
            Assert.NotEmpty(trades);
            Assert.All(trades, trade =>
            {
                Assert.Equal("BTCUSDT", trade.Pair);
                Assert.True(trade.Price > 0, "Price must be greater than 0");
                Assert.True(trade.Amount > 0, "Amount must be greater than 0");
            });
        }

        [Fact]
        public async Task GetCandleSeriesAsync_ShouldReturn_Candles()
        {
            var candles = await _connector.GetCandleSeriesAsync("BTCUSDT", 60, null, null, 5);

            Assert.NotNull(candles);
            Assert.NotEmpty(candles);
            Assert.All(candles, candle =>
            {
                Assert.Equal("BTCUSDT", candle.Pair);
                Assert.True(candle.OpenPrice > 0, "OpenPrice must be greater than 0");
                Assert.True(candle.ClosePrice > 0, "ClosePrice must be greater than 0");
                Assert.True(candle.HighPrice > 0, "HighPrice must be greater than 0");
                Assert.True(candle.LowPrice > 0, "LowPrice must be greater than 0");
            });
        }

        [Fact]
        public async Task WebSocket_ShouldReceive_Trades()
        {
            var receivedTrades = new List<Trade>();
            var tradeReceived = new TaskCompletionSource<bool>();

            _connector.NewBuyTrade += trade =>
            {
                receivedTrades.Add(trade);
                tradeReceived.TrySetResult(true);
            };

            _connector.NewSellTrade += trade =>
            {
                receivedTrades.Add(trade);
                tradeReceived.TrySetResult(true);
            };

            _connector.SubscribeTrades("BTCUSDT");

            // Ждем получения хотя бы одной сделки
            var completedTask = await Task.WhenAny(tradeReceived.Task, Task.Delay(60000)); // Увеличили время ожидания до 60 секунд

            _connector.UnsubscribeTrades("BTCUSDT");

            if (completedTask != tradeReceived.Task)
            {
                Assert.Fail("WebSocket не получил ни одной сделки в течение 60 секунд.");
            }

            Assert.NotEmpty(receivedTrades);
        }

        [Fact]
        public async Task WebSocket_ShouldReceive_Candles()
        {
            var receivedCandles = new List<Candle>();
            var candleReceived = new TaskCompletionSource<bool>();

            _connector.CandleSeriesProcessing += candle =>
            {
                receivedCandles.Add(candle);
                candleReceived.TrySetResult(true);
            };

            _connector.SubscribeCandles("BTCUSDT", 60);

            // Ждем получения хотя бы одной свечи
            var completedTask = await Task.WhenAny(candleReceived.Task, Task.Delay(60000)); // Увеличили время ожидания до 60 секунд

            _connector.UnsubscribeCandles("BTCUSDT");

            if (completedTask != candleReceived.Task)
            {
                Assert.Fail("WebSocket не получил ни одной свечи в течение 60 секунд.");
            }

            Assert.NotEmpty(receivedCandles);
        }
    }
}