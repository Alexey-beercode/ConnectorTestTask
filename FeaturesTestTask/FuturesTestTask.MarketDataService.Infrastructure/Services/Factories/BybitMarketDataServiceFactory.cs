using System.Net.Http;
using FeaturesTestTask.MarketDataService.Domain.Interfaces.Factories;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;
using Microsoft.Extensions.Options;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Factories;

public class BybitMarketDataServiceFactory : IMarketDataServiceFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<BybitOptions> _options;

    public BybitMarketDataServiceFactory(IHttpClientFactory httpClientFactory, IOptions<BybitOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public IMarketDataService CreateService()
    {
        var client = _httpClientFactory.CreateClient("BybitClient");
        return new BybitMarketDataService(client, _options);
    }
}