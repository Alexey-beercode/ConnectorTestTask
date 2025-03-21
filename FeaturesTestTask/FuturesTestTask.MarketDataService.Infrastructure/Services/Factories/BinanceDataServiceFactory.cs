using System.Net.Http;
using FeaturesTestTask.MarketDataService.Domain.Interfaces.Factories;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;
using Microsoft.Extensions.Options;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Factories;

public class BinanceDataServiceFactory : IMarketDataServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<BinanceOptions> _options;

    public BinanceDataServiceFactory(IHttpClientFactory httpClientFactory, IOptions<BinanceOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public IMarketDataService CreateService()
    {
        var client = _httpClientFactory.CreateClient("BinanceClient");
        return new BinanceMarketDataService(client, _options);
    }
}