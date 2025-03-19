using FeaturesTestTask.MarketDataService.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace FeaturesTestTask.MarketDataService.Infrastructure.Services.Decorators
{
    public class LoggingMarketDataService : IMarketDataService
    {
        private readonly IMarketDataService _innerService;
        private readonly ILogger<LoggingMarketDataService> _logger;

        public LoggingMarketDataService(IMarketDataService innerService, ILogger<LoggingMarketDataService> logger)
        {
            _innerService = innerService;
            _logger = logger;
        }

        public async Task<decimal> GetFuturesPriceAsync(string symbol)
        {
            _logger.LogInformation($"Fetching price for {symbol}");
            var price = await _innerService.GetFuturesPriceAsync(symbol);
            _logger.LogInformation($"Received price: {price}");
            return price;
        }
    }
}