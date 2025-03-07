using ConnectorTestTask.Core.Implementations;
using ConnectorTestTask.Core.Clients;
using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ConnectorTestTask.Tests;

public class BinanceConnectorTests : IAsyncLifetime
{
    private readonly ITestConnector _connector;
    private readonly IBinanceRestClient _restClient;
    private readonly IBinanceWebSocketClient _webSocketClient;
    private readonly string _testPair = "ETHUSDT";

    public BinanceConnectorTests()
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging()
            .BuildServiceProvider();

        _restClient = new BinanceRestClient(new HttpClient { BaseAddress = new Uri("https://api.binance.com/api/v3/") });
        _webSocketClient = new BinanceWebSocketClient(serviceProvider.GetRequiredService<ILogger<BinanceWebSocketClient>>());
        _connector = new BinanceConnector(_restClient, _webSocketClient);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _webSocketClient.DisconnectAsync();

    private async Task<T> RetryOnFailure<T>(Func<Task<T>> action, int retries = 5, int delayMs = 5000)
    {
        for (int i = 0; i < retries; i++)
        {
            var result = await action();
            if (result is IEnumerable<object> collection && collection.Any())
                return result;
            await Task.Delay(delayMs);
        }
        return await action();
    }

    [Fact]
    public async Task GetNewTradesAsync_ShouldReturn_Trades()
    {
        var trades = await RetryOnFailure(() => _connector.GetNewTradesAsync(_testPair, 5));
        Assert.NotEmpty(trades);
    }

    [Fact]
    public async Task GetCandleSeriesAsync_ShouldReturn_Candles()
    {
        var candles = await RetryOnFailure(() => _connector.GetCandleSeriesAsync(_testPair, 60, null, null, 5));
        Assert.NotEmpty(candles);
    }

    [Fact]
    public async Task WebSocket_ShouldReceive_Trades()
    {
        var receivedTrades = new List<Trade>();
        var tradeReceivedEvent = new TaskCompletionSource<bool>();

        _connector.NewBuyTrade += trade =>
        {
            receivedTrades.Add(trade);
            if (receivedTrades.Count >= 3) tradeReceivedEvent.TrySetResult(true);
        };

        _connector.NewSellTrade += trade =>
        {
            receivedTrades.Add(trade);
            if (receivedTrades.Count >= 3) tradeReceivedEvent.TrySetResult(true);
        };

        await _webSocketClient.SubscribeTrades(_testPair);
        var completedTask = await Task.WhenAny(tradeReceivedEvent.Task, Task.Delay(TimeSpan.FromMinutes(1)));

        await _webSocketClient.Unsubscribe(_testPair);
        Assert.True(completedTask == tradeReceivedEvent.Task);
    }

    [Fact]
    public async Task WebSocket_ShouldReceive_Candles()
    {
        var receivedCandles = new List<Candle>();
        var candleReceivedEvent = new TaskCompletionSource<bool>();

        _connector.CandleSeriesProcessing += candle =>
        {
            receivedCandles.Add(candle);
            if (receivedCandles.Count >= 3) candleReceivedEvent.TrySetResult(true);
        };

        await _webSocketClient.SubscribeCandles(_testPair, 60);
        var completedTask = await Task.WhenAny(candleReceivedEvent.Task, Task.Delay(TimeSpan.FromMinutes(1)));

        await _webSocketClient.Unsubscribe(_testPair);
        Assert.True(completedTask == candleReceivedEvent.Task);
    }

    [Fact]
    public async Task GetAvailableCurrenciesAsync_ShouldReturn_Currencies()
    {
        var currencies = await _connector.GetAvailableCurrenciesAsync();
        Assert.Contains("BTC", currencies);
        Assert.Contains("USDT", currencies);
    }

    [Fact]
    public async Task GetCandleSeries_WithDates_ShouldReturn_Candles()
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-10);
        var to = DateTimeOffset.UtcNow;
        var candles = await _connector.GetCandleSeriesAsync(_testPair, 60, from, to, 5);
        Assert.NotEmpty(candles);
    }

    [Fact]
    public async Task WebSocket_ShouldReceive_Multiple_Trades()
    {
        var receivedTrades = new List<Trade>();
        var tradeReceivedEvent = new TaskCompletionSource<bool>();

        _connector.NewBuyTrade += trade =>
        {
            receivedTrades.Add(trade);
            if (receivedTrades.Count >= 3) tradeReceivedEvent.TrySetResult(true);
        };

        _connector.NewSellTrade += trade =>
        {
            receivedTrades.Add(trade);
            if (receivedTrades.Count >= 3) tradeReceivedEvent.TrySetResult(true);
        };

        await _webSocketClient.SubscribeTrades(_testPair);
        var completedTask = await Task.WhenAny(tradeReceivedEvent.Task, Task.Delay(TimeSpan.FromMinutes(1)));

        await _webSocketClient.Unsubscribe(_testPair);
        Assert.True(receivedTrades.Count >= 3);
    }

    [Fact]
    public async Task WebSocket_ShouldReceive_Multiple_Candles()
    {
        var receivedCandles = new List<Candle>();
        var candleReceivedEvent = new TaskCompletionSource<bool>();

        _connector.CandleSeriesProcessing += candle =>
        {
            receivedCandles.Add(candle);
            if (receivedCandles.Count >= 3) candleReceivedEvent.TrySetResult(true);
        };

        await _webSocketClient.SubscribeCandles(_testPair, 60);
        var completedTask = await Task.WhenAny(candleReceivedEvent.Task, Task.Delay(TimeSpan.FromMinutes(1)));

        await _webSocketClient.Unsubscribe(_testPair);
        Assert.True(receivedCandles.Count >= 3);
    }

    [Fact]
    public async Task GetNewTradesAsync_ShouldThrowException_OnInvalidPair()
    {
        await Assert.ThrowsAsync<HttpRequestException>(() => _connector.GetNewTradesAsync("INVALIDPAIR", 5));
    }
    
}
