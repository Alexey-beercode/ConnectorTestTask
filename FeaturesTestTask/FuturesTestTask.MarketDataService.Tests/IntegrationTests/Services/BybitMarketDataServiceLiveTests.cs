using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;
using Microsoft.Extensions.Options;

namespace FuturesTestTask.MarketDataService.Tests.IntegrationTests.Services;

public class BybitMarketDataServiceLiveTests
{
    private readonly IMarketDataService _service;

    public BybitMarketDataServiceLiveTests()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.bybit.com")
        };

        var options = Options.Create(new BybitOptions
        {
            FuturesApiBaseUrl = "https://api.bybit.com/v5/market"
        });

        _service = new BybitMarketDataService(httpClient, options);
    }

    [Fact(DisplayName = "Live Bybit API returns actual close price for BTCUSDT_QUARTER")]
    public async Task GetFuturesClosePriceAsync_LiveRequest_ReturnsValidPrice()
    {
        // Arrange
        var symbol = "BTCUSDT"; 
        var interval = "1d";
        var date = DateTime.UtcNow.AddDays(-1); 

        // Act
        var price = await _service.GetFuturesClosePriceAsync(symbol, interval, date);

        // Assert
        Assert.NotNull(price);
        Assert.True(price > 0);
    }

    [Fact(DisplayName = "Live Bybit API returns null for invalid symbol")]
    public async Task GetFuturesClosePriceAsync_LiveRequest_InvalidSymbol_ReturnsNull()
    {
        // Arrange
        var invalidSymbol = "INVALID123";

        // Act
        var price = await _service.GetFuturesClosePriceAsync(invalidSymbol, "1d", DateTime.UtcNow.AddDays(-1));

        // Assert
        Assert.Null(price);
    }

    [Fact(DisplayName = "Live Bybit API handles hourly interval")]
    public async Task GetFuturesClosePriceAsync_HourlyInterval_WorksCorrectly()
    {
        var symbol = "BTCUSDT";
        var interval = "1h";
        var date = DateTime.UtcNow.AddHours(-2);

        var price = await _service.GetFuturesClosePriceAsync(symbol, interval, date);

        Assert.NotNull(price);
        Assert.True(price > 0);
    }
}
