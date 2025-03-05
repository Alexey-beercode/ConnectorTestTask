using ConnectorTestTask.Core.Interfaces;

namespace ConnectorTestTask.Core.Implementations;

public class CurrencyConverter:ICurrencyConverter
{
    private readonly IBinanceRestClient _restClient;

    public CurrencyConverter(IBinanceRestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task<decimal> ConvertToUsdtAsync(string currency, decimal amount)
    {
        if (currency == "USDT") return amount;

        decimal rateToUsdt = await _restClient.GetTickerAsync($"{currency}USDT");
        if (rateToUsdt == 0)
        {
            throw new InvalidOperationException($"Не найден курс {currency} → USDT.");
        }

        return amount * rateToUsdt;
    }

    public async Task<decimal> ConvertFromUsdtAsync(string targetCurrency, decimal amountInUsdt)
    {
        if (targetCurrency == "USDT") return amountInUsdt;

        decimal targetRate = await _restClient.GetTickerAsync($"{targetCurrency}USDT");
        if (targetRate == 0)
        {
            throw new InvalidOperationException($"Не найден курс USDT → {targetCurrency}.");
        }

        return amountInUsdt / targetRate;
    }
}