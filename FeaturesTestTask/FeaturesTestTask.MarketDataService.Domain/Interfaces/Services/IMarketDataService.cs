namespace FeaturesTestTask.MarketDataService.Domain.Interfaces.Services;

public interface IMarketDataService
{
    Task<decimal> GetFuturesPriceAsync(string symbol);
}