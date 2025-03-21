namespace FuturesTestTask.MarketDataService.Domain.Interfaces.Services;

public interface IMarketDataService
{
    Task<decimal?> GetFuturesClosePriceAsync(string symbol, string interval, DateTime dateUtc);
}