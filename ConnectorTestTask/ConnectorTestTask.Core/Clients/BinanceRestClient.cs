using System.Globalization;
using System.Text.Json;
using ConnectorTestTask.Core.Helpers;
using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Clients
{
    public class BinanceRestClient : IBinanceRestClient
    {
        private readonly HttpClient _httpClient;

        public BinanceRestClient(HttpClient httpClient)
        {
            var baseUri = new Uri("https://api.binance.com/api/v3/");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = baseUri;
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
                string endpoint = $"klines?symbol={pair.ToUpper()}&interval={interval}&startTime={from?.ToUnixTimeMilliseconds()}&endTime={to?.ToUnixTimeMilliseconds()}&limit={count}";
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
                using var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    return 0;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);

                if (json != null && json.TryGetValue("price", out var priceStr))
                {
                    if (decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                    {
                        return price;
                    }
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<HashSet<string>> GetAvailableCurrenciesAsync()
        {
            try
            {
                string endpoint = "exchangeInfo";
                var response = await _httpClient.GetStringAsync(endpoint);
                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(response);

                var symbols = json["symbols"] as JsonElement?;
                var availableCurrencies = new HashSet<string>();

                foreach (var symbol in symbols.Value.EnumerateArray())
                {
                    availableCurrencies.Add(symbol.GetProperty("baseAsset").GetString());
                    availableCurrencies.Add(symbol.GetProperty("quoteAsset").GetString());
                }

                return availableCurrencies;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения списка валют: {ex.Message}");
                return new HashSet<string>();
            }
        }
    }
}
