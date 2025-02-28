using System.Text.Json;
using ConnectorTestTask.Core.Helpers;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Clients
{
    public class BinanceRestClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.binance.com/api/v3/";

        public BinanceRestClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int maxCount)
        {
            string endpoint = $"trades?symbol={pair.ToUpper()}&limit={maxCount}";
            var response = await _httpClient.GetStringAsync(endpoint);
            return BinanceParser.ParseTrades(response, pair);
        }
        
        public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            string interval = TimeConverter.ConvertToTimeframe(periodInSec);
            string endpoint = $"klines?symbol={pair.ToUpper()}&interval={interval}&limit={count}";
            var response = await _httpClient.GetStringAsync(endpoint);
            return BinanceParser.ParseCandles(response, pair);
        }
        
        public async Task<decimal> GetTickerAsync(string pair)
        {
            string endpoint = $"ticker/price?symbol={pair.ToUpper()}";
            var response = await _httpClient.GetStringAsync(endpoint);
            var json = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
            return json != null ? Convert.ToDecimal(json["price"]) : 0;
        }
    }
}
