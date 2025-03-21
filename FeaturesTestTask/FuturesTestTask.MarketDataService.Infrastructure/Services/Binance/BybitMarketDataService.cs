using System.Globalization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Models;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;

public class BybitMarketDataService : IMarketDataService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public BybitMarketDataService(HttpClient httpClient, IOptions<BybitOptions> options)
    {
        _httpClient = httpClient;
        _baseUrl = options.Value.FuturesApiBaseUrl.TrimEnd('/');
    }

    public async Task<decimal?> GetFuturesClosePriceAsync(string symbol, string interval, DateTime dateUtc)
    {
        var roundedDate = RoundToIntervalStart(dateUtc, interval);
        var timestamp = new DateTimeOffset(roundedDate).ToUnixTimeSeconds();

        var requestUrl = $"{_baseUrl}/kline?symbol={symbol}&interval={MapInterval(interval)}&from={timestamp}&category=linear";
        var response = await _httpClient.GetStringAsync(requestUrl);

        var result = JsonConvert.DeserializeObject<BybitKlineResponse>(response);
        if (result?.Result?.List?.Count > 0)
        {
            return decimal.Parse(result.Result.List[0][4], CultureInfo.InvariantCulture);

        }

        return null;
    }

    private static string MapInterval(string interval) => interval switch
    {
        "1d" => "D",
        "1h" => "60",
        _ => throw new ArgumentException($"Unsupported interval: '{interval}'", nameof(interval))
    };

    private static DateTime RoundToIntervalStart(DateTime dateUtc, string interval) =>
        interval switch
        {
            "1d" => dateUtc.Date,
            "1h" => new DateTime(dateUtc.Year, dateUtc.Month, dateUtc.Day, dateUtc.Hour, 0, 0, DateTimeKind.Utc),
            _ => throw new ArgumentException($"Unsupported interval: '{interval}'", nameof(interval))
        };
}
