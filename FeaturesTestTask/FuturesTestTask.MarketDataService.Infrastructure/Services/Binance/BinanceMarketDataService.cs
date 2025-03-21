using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using Newtonsoft.Json;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;

public class BinanceMarketDataService : IMarketDataService
{
    private const string BinanceFuturesBaseUrl = "https://fapi.binance.com/fapi/v1/klines";

    private readonly HttpClient _httpClient;

    public BinanceMarketDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<decimal?> GetFuturesClosePriceAsync(string symbol, string interval, DateTime dateUtc)
    {
        var roundedDate = RoundToIntervalStart(dateUtc, interval);
        var startTime = new DateTimeOffset(roundedDate).ToUnixTimeMilliseconds();

        var requestUrl = $"{BinanceFuturesBaseUrl}?symbol={symbol}&interval={interval}&startTime={startTime}&limit=1";

        var response = await _httpClient.GetStringAsync(requestUrl);
        var klineData = JsonConvert.DeserializeObject<List<List<object>>>(response);

        return ExtractClosePrice(klineData);
    }

    private static DateTime RoundToIntervalStart(DateTime dateUtc, string interval)
    {
        return interval switch
        {
            "1d" => dateUtc.Date,
            "1h" => new DateTime(dateUtc.Year, dateUtc.Month, dateUtc.Day, dateUtc.Hour, 0, 0, DateTimeKind.Utc),
            _ => throw new ArgumentException($"Unsupported interval: '{interval}'", nameof(interval))
        };
    }

    private static decimal? ExtractClosePrice(List<List<object>>? klineData)
    {
        if (klineData is null || klineData.Count == 0)
            return null;

        var closeValue = klineData[0][4]?.ToString();
        return decimal.TryParse(closeValue, out var result) ? result : null;
    }
}