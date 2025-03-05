using System.Text.Json;
using ConnectorTestTask.Core.Clients;
using ConnectorTestTask.Core.Implementations;
using ConnectorTestTask.Core.Models;
using MockServer.Client.Net;
using MockServer.Client.Net.Models;

namespace ConnectorTestTask.Tests
{
    public class BinanceConnectorTests : IAsyncLifetime
    {
        private readonly BinanceConnector _connector;
        private readonly BinanceRestClient _restClient;
        private readonly BinanceWebSocketClient _webSocketClient;
        private readonly HttpClient _httpClient;
        private readonly MockServerClient _mockServer;

        public BinanceConnectorTests()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:1080") };
            _restClient = new BinanceRestClient(_httpClient);
            _webSocketClient = new BinanceWebSocketClient();
            _connector = new BinanceConnector(_restClient, _webSocketClient);
            _mockServer = new MockServerClient("http://localhost:1080");
        }

        public async Task InitializeAsync()
        {
            await SetupMockResponses();
        }

        public async Task DisposeAsync()
        {
            // MockServer автоматически завершается, ничего вызывать не нужно
        }

        private async Task SetupMockResponses()
        {
            var expectation = new ExpectationDTO
            {
                HttpRequest = new HttpRequestDTO
                {
                    Method = "GET",
                    Path = "/api/v3/exchangeInfo"
                },
                HttpResponse = new HttpResponseDTO
                {
                    Body = JsonSerializer.Serialize(new
                    {
                        symbols = new[]
                        {
                            new { symbol = "BTCUSDT", baseAsset = "BTC", quoteAsset = "USDT" },
                            new { symbol = "ETHUSDT", baseAsset = "ETH", quoteAsset = "USDT" }
                        }
                    }),
                    StatusCode = 200
                }
            };
            await _mockServer.ExpectationAsync(expectation);
        }

        [Fact]
        public async Task GetAvailableCurrenciesAsync_Returns_CorrectList()
        {
            var result = await _connector.GetAvailableCurrenciesAsync();

            Assert.NotNull(result);
            Assert.Contains("BTC", result);
            Assert.Contains("ETH", result);
            Assert.Contains("USDT", result);
        }

        [Fact]
        public async Task GetNewTradesAsync_Returns_Trades()
        {
            var mockTrades = new List<Trade>
            {
                new Trade { Pair = "BTCUSDT", Price = 50000, Amount = 0.1M, Side = "buy", Time = DateTimeOffset.UtcNow }
            };

            _mockServer
                .When(HttpRequest.Request()
                    .WithMethod("GET")
                    .WithPath("/api/v3/trades"))
                .Respond(HttpResponse.Response()
                    .WithBody(JsonSerializer.Serialize(mockTrades))
                    .WithStatusCode(200));

            var result = await _connector.GetNewTradesAsync("BTCUSDT", 5);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, trade => Assert.Equal("BTCUSDT", trade.Pair));
        }

        [Fact]
        public async Task GetCandleSeriesAsync_Returns_Candles()
        {
            var mockCandles = new List<List<object>>
            {
                new List<object> { 1631234567000, "50000", "51000", "49000", "50500", "1000" },
                new List<object> { 1631234600000, "50500", "51500", "49500", "51000", "1200" }
            };

            _mockServer
                .When(HttpRequest.Request()
                    .WithMethod("GET")
                    .WithPath("/api/v3/klines"))
                .Respond(HttpResponse.Response()
                    .WithBody(JsonSerializer.Serialize(mockCandles))
                    .WithStatusCode(200));

            var result = await _connector.GetCandleSeriesAsync("BTCUSDT", 60, null, null, 2);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.All(result, candle => Assert.Equal("BTCUSDT", candle.Pair));
        }

        [Fact]
        public async Task CalculatePortfolioValueAsync_Calculates_Correctly()
        {
            var portfolio = new Dictionary<string, decimal>
            {
                { "BTC", 1 },
                { "ETH", 10 }
            };

            var result = await _connector.CalculatePortfolioValueAsync(portfolio, "USDT");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(50000, result["BTC"]);
        }

        [Fact]
        public async Task WebSocketClient_Receives_Trades()
        {
            var receivedTrades = new List<Trade>();
            _connector.NewBuyTrade += trade => receivedTrades.Add(trade);
            _connector.NewSellTrade += trade => receivedTrades.Add(trade);

            await _webSocketClient.ConnectAsync();
            await _webSocketClient.SubscribeTrades("BTCUSDT");

            await Task.Delay(5000);

            Assert.NotEmpty(receivedTrades);
        }
    }
}
