using System.Net.Http;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Common;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;
using Microsoft.Extensions.Options;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Factories;

public class BinanceDataServiceFactory : MarketDataServiceFactory
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<BinanceOptions> _options;

    public BinanceDataServiceFactory(HttpClient httpClient, IOptions<BinanceOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public override IMarketDataService CreateService()
    {
        return new BinanceMarketDataService(_httpClient, _options);
    }
}