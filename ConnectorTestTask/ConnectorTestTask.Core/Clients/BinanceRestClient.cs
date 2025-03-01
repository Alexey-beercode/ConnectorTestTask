using System.Text.Json;
using ConnectorTestTask.Core.Helpers;
using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Clients
{
    public class BinanceRestClient : IBinanceRestClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.binance.com/api/v3/";

        public BinanceRestClient()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }

        public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int maxCount)
        {
            try
            {
                string endpoint = $"trades?symbol={pair.ToUpper()}&limit={maxCount}";
                var response = await _httpClient.GetStringAsync(endpoint);
                return BinanceParser.ParseTrades(response, pair);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения трейдов: {ex.Message}");
                return new List<Trade>();
            }
        }

        public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            try
            {
                string interval = TimeConverter.ConvertToTimeframe(periodInSec);
                string endpoint = $"klines?symbol={pair.ToUpper()}&interval={interval}&limit={count}";
                var response = await _httpClient.GetStringAsync(endpoint);
                return BinanceParser.ParseCandles(response, pair);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения свечей: {ex.Message}");
                return new List<Candle>();
            }
        }

        public async Task<decimal> GetTickerAsync(string pair)
        {
            try
            {
                string endpoint = $"ticker/price?symbol={pair.ToUpper()}";
                var response = await _httpClient.GetStringAsync(endpoint);
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
                return json != null ? Convert.ToDecimal(json["price"]) : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения тикера: {ex.Message}");
                return 0;
            }
        }
    }
}
