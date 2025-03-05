namespace ConnectorTestTask.Core.Interfaces;

public interface ICurrencyConverter
{
    Task<decimal> ConvertToUsdtAsync(string currency, decimal amount);
    Task<decimal> ConvertFromUsdtAsync(string targetCurrency, decimal amountInUsdt);
}