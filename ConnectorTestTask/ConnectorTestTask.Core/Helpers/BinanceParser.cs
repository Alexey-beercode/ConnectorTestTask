using System.Text.Json;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Helpers;

public static class BinanceParser
{
    public static IEnumerable<Trade> ParseTrades(string json, string pair)
    {
        var tradesList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
        var trades = new List<Trade>();

        if (tradesList == null) return trades;

        foreach (var trade in tradesList)
        {
            trades.Add(new Trade
            {
                Id = trade["id"].ToString(),
                Time = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(trade["time"])),
                Amount = Convert.ToDecimal(trade["qty"]),
                Price = Convert.ToDecimal(trade["price"]),
                Pair = pair,
                Side = Convert.ToBoolean(trade["isBuyerMaker"]) ? "sell" : "buy"
            });
        }

        return trades;
    }
    
    public static IEnumerable<Candle> ParseCandles(string json, string pair)
    {
        var candlesList = JsonSerializer.Deserialize<List<List<object>>>(json);
        var candles = new List<Candle>();

        if (candlesList == null) return candles;

        foreach (var candle in candlesList)
        {
            candles.Add(new Candle
            {
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(candle[0])),
                OpenPrice = Convert.ToDecimal(candle[1]),
                HighPrice = Convert.ToDecimal(candle[2]),
                LowPrice = Convert.ToDecimal(candle[3]),
                ClosePrice = Convert.ToDecimal(candle[4]),
                TotalVolume = Convert.ToDecimal(candle[5]),
                Pair = pair
            });
        }

        return candles;
    }
}