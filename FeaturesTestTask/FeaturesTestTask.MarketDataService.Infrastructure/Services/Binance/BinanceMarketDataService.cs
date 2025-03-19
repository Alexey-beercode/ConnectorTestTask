using FeaturesTestTask.MarketDataService.Domain.Interfaces.Services;

namespace FeaturesTestTask.MarketDataService.Infrastructure.Services.Binance;

public class BinanceMarketDataService : IMarketDataService
{
    private readonly HttpClient _httpClient;

    public BinanceMarketDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public Task<decimal> GetFuturesPriceAsync(string symbol)
    {
        throw new NotImplementedException();
    }
}