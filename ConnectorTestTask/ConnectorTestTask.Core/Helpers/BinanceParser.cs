using System.Text.Json;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Helpers;

public static class BinanceParser
{
    public static IEnumerable<Trade> ParseTrades(string json, string pair)
    {
        var tradesList = JsonSerializer.Deserialize<JsonElement>(json);

        if (tradesList.ValueKind != JsonValueKind.Array)
            throw new JsonException("Неверный формат данных для трейдов.");

        var trades = new List<Trade>();

        foreach (var trade in tradesList.EnumerateArray())
        {
            trades.Add(new Trade
            {
                Id = trade.GetProperty("id").ToString(),
                Time = DateTimeOffset.FromUnixTimeMilliseconds(trade.GetProperty("time").GetInt64()),
                Amount = decimal.Parse(trade.GetProperty("qty").GetString(), System.Globalization.CultureInfo.InvariantCulture),
                Price = decimal.Parse(trade.GetProperty("price").GetString(), System.Globalization.CultureInfo.InvariantCulture),
                Pair = pair,
                Side = trade.GetProperty("isBuyerMaker").GetBoolean() ? "sell" : "buy"
            });
        }

        if (trades.Count == 0)
            throw new JsonException("Парсер не смог обработать трейды. Binance API вернул пустой список.");

        return trades;
    }

    public static IEnumerable<Candle> ParseCandles(string json, string pair)
    {
        var candlesList = JsonSerializer.Deserialize<JsonElement>(json);

        if (candlesList.ValueKind != JsonValueKind.Array)
            throw new JsonException("Неверный формат данных для свечей.");

        var candles = new List<Candle>();

        foreach (var candle in candlesList.EnumerateArray())
        {
            candles.Add(new Candle
            {
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(candle[0].GetInt64()),
                OpenPrice = decimal.Parse(candle[1].GetString(), System.Globalization.CultureInfo.InvariantCulture),
                HighPrice = decimal.Parse(candle[2].GetString(), System.Globalization.CultureInfo.InvariantCulture),
                LowPrice = decimal.Parse(candle[3].GetString(), System.Globalization.CultureInfo.InvariantCulture),
                ClosePrice = decimal.Parse(candle[4].GetString(), System.Globalization.CultureInfo.InvariantCulture),
                TotalVolume = decimal.Parse(candle[5].GetString(), System.Globalization.CultureInfo.InvariantCulture),
                Pair = pair
            });
        }

        if (candles.Count == 0)
            throw new JsonException("Парсер не смог обработать свечи. Binance API вернул пустой список.");

        return candles;
    }
    public static Trade ParseTrade(JsonElement json, string pair)
    {
        return new Trade
        {
            Id = json.GetProperty("t").ToString(),
            Time = DateTimeOffset.FromUnixTimeMilliseconds(json.GetProperty("E").GetInt64()),
            Amount = decimal.Parse(json.GetProperty("q").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            Price = decimal.Parse(json.GetProperty("p").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            Pair = pair,
            Side = json.GetProperty("m").GetBoolean() ? "sell" : "buy"
        };
    }
    
    public static Candle ParseCandle(JsonElement json, string pair)
    {
        var kline = json.GetProperty("k");

        return new Candle
        {
            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(kline.GetProperty("t").GetInt64()),
            OpenPrice = decimal.Parse(kline.GetProperty("o").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            HighPrice = decimal.Parse(kline.GetProperty("h").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            LowPrice = decimal.Parse(kline.GetProperty("l").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            ClosePrice = decimal.Parse(kline.GetProperty("c").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            TotalVolume = decimal.Parse(kline.GetProperty("v").GetString(), System.Globalization.CultureInfo.InvariantCulture),
            Pair = pair
        };
    }
}
