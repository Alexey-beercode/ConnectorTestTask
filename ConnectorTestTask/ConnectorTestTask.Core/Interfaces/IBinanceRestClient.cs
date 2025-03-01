using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Interfaces;

public interface IBinanceRestClient
{
    Task<IEnumerable<Trade>> GetTradesAsync(string pair, int maxCount);
    Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);
    Task<decimal> GetTickerAsync(string pair);
    Task<HashSet<string>> GetAvailableCurrenciesAsync();
}