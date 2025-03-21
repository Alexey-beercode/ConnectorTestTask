using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Models;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace FuturesTestTask.MarketDataService.Tests.IntegrationTests.Services;

public class BybitMarketDataServiceTests
{
    private IMarketDataService CreateService(decimal closePrice)
    {
        var klineResponse = new BybitKlineResponse
        {
            RetCode = 0,
            RetMsg = "OK",
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Result = new BybitKlineResult
            {
                Symbol = "BTCUSDT",
                Category = "linear",
                List = new List<List<string>>
                {
                    new() { "1710806400000", "67643.9", "68174.9", "61555", closePrice.ToString(System.Globalization.CultureInfo.InvariantCulture), "270938.291", "17413254152.0052" }
                }
            }
        };

        var json = JsonConvert.SerializeObject(klineResponse);

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://mocked.bybit.com")
        };

        var options = Options.Create(new BybitOptions
        {
            FuturesApiBaseUrl = "https://mocked.bybit.com"
        });

        return new BybitMarketDataService(httpClient, options);
    }

    [Fact]
    public async Task GetFuturesClosePriceAsync_ReturnsCorrectPrice()
    {
        // Arrange
        var expectedPrice = 1050.50m;
        var service = CreateService(expectedPrice);

        // Act
        var price = await service.GetFuturesClosePriceAsync("BTCUSDT", "1d", DateTime.UtcNow);

        // Assert
        Assert.NotNull(price);
        Assert.Equal(expectedPrice, price);
    }

    [Fact]
    public async Task GetFuturesClosePriceAsync_ReturnsNull_WhenListEmpty()
    {
        var emptyResponse = new BybitKlineResponse
        {
            RetCode = 0,
            RetMsg = "OK",
            Result = new BybitKlineResult
            {
                Symbol = "BTCUSDT",
                Category = "linear",
                List = new List<List<string>>() // пусто
            },
            RetExtInfo = new(),
            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        var json = JsonConvert.SerializeObject(emptyResponse);

        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://mocked.bybit.com")
        };

        var options = Options.Create(new BybitOptions
        {
            FuturesApiBaseUrl = "https://mocked.bybit.com"
        });

        var service = new BybitMarketDataService(httpClient, options);

        // Act
        var price = await service.GetFuturesClosePriceAsync("BTCUSDT", "1d", DateTime.UtcNow);

        // Assert
        Assert.Null(price);
    }

    [Fact]
    public async Task GetFuturesClosePriceAsync_ThrowsException_OnInvalidInterval()
    {
        // Arrange
        var service = CreateService(1000);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetFuturesClosePriceAsync("BTCUSDT", "5m", DateTime.UtcNow));
    }
}
