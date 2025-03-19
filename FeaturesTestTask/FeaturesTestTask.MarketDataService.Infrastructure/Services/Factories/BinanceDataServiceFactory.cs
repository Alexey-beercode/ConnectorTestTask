using FeaturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FeaturesTestTask.MarketDataService.Infrastructure.Services.Binance;

namespace FeaturesTestTask.MarketDataService.Infrastructure.Services.Factories;

public class BinanceDataServiceFactory : MarketDataServiceFactory
{
    private readonly HttpClient _httpClient;

    public BinanceDataServiceFactory(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override IMarketDataService CreateService()
    {
        return new BinanceMarketDataService(_httpClient);
    }
}