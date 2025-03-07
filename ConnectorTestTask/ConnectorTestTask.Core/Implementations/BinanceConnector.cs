using ConnectorTestTask.Core.Interfaces;
using ConnectorTestTask.Core.Models;

namespace ConnectorTestTask.Core.Implementations;

public class BinanceConnector : ITestConnector
{
    private readonly IBinanceRestClient _restClient;
    private readonly IBinanceWebSocketClient _webSocketClient;
    private readonly CurrencyConverter _converter;

    public event Action<Trade> NewBuyTrade;
    public event Action<Trade> NewSellTrade;
    public event Action<Candle> CandleSeriesProcessing;

    public BinanceConnector(IBinanceRestClient restClient, IBinanceWebSocketClient webSocketClient)
    {
        _restClient = restClient;
        _webSocketClient = webSocketClient;
        _converter = new CurrencyConverter(_restClient);

        _webSocketClient.NewTradeReceived += trade =>
        {
            if (trade.Side == "buy") NewBuyTrade?.Invoke(trade);
            else NewSellTrade?.Invoke(trade);
        };

        _webSocketClient.NewCandleReceived += candle => CandleSeriesProcessing?.Invoke(candle);
    }

    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount) =>
        await _restClient.GetTradesAsync(pair, maxCount);

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
        string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0) =>
        await _restClient.GetCandlesAsync(pair, periodInSec, from, to, count);

    public async Task<HashSet<string>> GetAvailableCurrenciesAsync() =>
        await _restClient.GetAvailableCurrenciesAsync();

    public async Task<Dictionary<string, decimal>> CalculatePortfolioValueAsync(
        Dictionary<string, decimal> portfolio, string targetCurrency)
    {
        var (portfolioInUsdt, totalUsdtValue, totalTargetValue) =
            await ConvertPortfolioToUsdtAsync(portfolio, targetCurrency);

        if (targetCurrency == "USDT")
        {
            portfolioInUsdt["Total"] = totalUsdtValue + totalTargetValue;
            return portfolioInUsdt;
        }

        totalTargetValue += await ConvertUsdtToTargetAsync(totalUsdtValue, targetCurrency);
        return await RecalculatePortfolioToTargetAsync(portfolioInUsdt, targetCurrency, totalTargetValue);
    }

    private async Task<(Dictionary<string, decimal>, decimal, decimal)> ConvertPortfolioToUsdtAsync(
        Dictionary<string, decimal> portfolio, string targetCurrency)
    {
        var portfolioInUsdt = new Dictionary<string, decimal>();
        decimal totalUsdtValue = 0, totalTargetValue = 0;

        foreach (var (currency, amount) in portfolio)
        {
            if (currency == targetCurrency)
            {
                portfolioInUsdt[currency] = amount;
                totalTargetValue += amount;
                continue;
            }

            decimal converted = await _converter.ConvertToUsdtAsync(currency, amount);
            portfolioInUsdt[currency] = converted;
            totalUsdtValue += converted;
        }

        return (portfolioInUsdt, totalUsdtValue, totalTargetValue);
    }

    private async Task<decimal> ConvertUsdtToTargetAsync(decimal totalUsdtValue, string targetCurrency)
    {
        decimal targetRate = await _converter.ConvertFromUsdtAsync(targetCurrency, totalUsdtValue);
        if (targetRate == 0) throw new InvalidOperationException($"Ошибка: Не найден курс USDT → {targetCurrency}");
        return targetRate;
    }

    private async Task<Dictionary<string, decimal>> RecalculatePortfolioToTargetAsync(
        Dictionary<string, decimal> portfolioInUsdt, string targetCurrency, decimal totalTargetValue)
    {
        var finalPortfolioValue = new Dictionary<string, decimal>();

        foreach (var (currency, valueInUsdt) in portfolioInUsdt)
            finalPortfolioValue[currency] = currency == targetCurrency
                ? valueInUsdt
                : await _converter.ConvertFromUsdtAsync(targetCurrency, valueInUsdt);

        finalPortfolioValue["Total"] = totalTargetValue;
        return finalPortfolioValue;
    }

    public async void SubscribeTrades(string pair, int maxCount = 100)
    {
        await _webSocketClient.ConnectAsync();
        await _webSocketClient.SubscribeTrades(pair);
    }

    public void UnsubscribeTrades(string pair) => _webSocketClient.Unsubscribe(pair, "1m");

    public async void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null,
        DateTimeOffset? to = null, long? count = 0)
    {
        await _webSocketClient.ConnectAsync();
        await _webSocketClient.SubscribeCandles(pair, periodInSec);
    }

    public void UnsubscribeCandles(string pair) => _webSocketClient.Unsubscribe(pair, "1m");
}
