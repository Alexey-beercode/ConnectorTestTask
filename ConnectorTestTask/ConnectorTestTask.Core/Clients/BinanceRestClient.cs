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
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.BaseAddress = new Uri("https://api.binance.com/api/v3/");
    }

    public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, int maxCount)
    {
        var response = await _httpClient.GetAsync($"trades?symbol={pair.ToUpper()}&limit={maxCount}");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return BinanceParser.ParseTrades(responseBody, pair);
    }

    public async Task<IEnumerable<Candle>> GetCandlesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
    {
        var interval = TimeConverter.ConvertToTimeframe(periodInSec);
        var response = await _httpClient.GetAsync($"klines?symbol={pair.ToUpper()}&interval={interval}&limit={count}");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return BinanceParser.ParseCandles(responseBody, pair);
    }

    public async Task<decimal> GetTickerAsync(string pair)
    {
        var response = await _httpClient.GetAsync($"ticker/price?symbol={pair.ToUpper()}");
        if (!response.IsSuccessStatusCode) return 0;
        var json = JsonSerializer.Deserialize<Dictionary<string, string>>(await response.Content.ReadAsStringAsync());
        return json?.TryGetValue("price", out var priceStr) == true && decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var price) ? price : 0;
    }

    public async Task<HashSet<string>> GetAvailableCurrenciesAsync()
    {
        var response = await _httpClient.GetStringAsync("exchangeInfo");
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
        var symbols = json?["symbols"] as JsonElement?;
        var availableCurrencies = new HashSet<string>();
        foreach (var symbol in symbols?.EnumerateArray() ?? Enumerable.Empty<JsonElement>())
        {
            availableCurrencies.Add(symbol.GetProperty("baseAsset").GetString());
            availableCurrencies.Add(symbol.GetProperty("quoteAsset").GetString());
        }
        return availableCurrencies;
    }
}
}
