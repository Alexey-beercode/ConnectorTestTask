using System;
using System.Threading.Tasks;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Decorators;

public class LoggingMarketDataService : IMarketDataService
{
    private readonly IMarketDataService _inner;
    private readonly ILogger<LoggingMarketDataService> _logger;

    public LoggingMarketDataService(
        IMarketDataService inner,
        ILogger<LoggingMarketDataService> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public async Task<decimal?> GetFuturesClosePriceAsync(string symbol, string interval, DateTime dateUtc)
    {
        _logger.LogInformation("Запрос цены: {Symbol}, интервал: {Interval}, дата: {Date}",
            symbol, interval, dateUtc.ToString("u"));

        var price = await _inner.GetFuturesClosePriceAsync(symbol, interval, dateUtc);

        if (price is not null)
        {
            _logger.LogInformation("Цена закрытия получена: {Price} {Symbol} ({Interval}) @ {Date}",
                price, symbol, interval, dateUtc.ToString("u"));
        }
        else
        {
            _logger.LogWarning("Цена отсутствует: {Symbol}, интервал: {Interval}, дата: {Date}",
                symbol, interval, dateUtc.ToString("u"));
        }

        return price;
    }
}