using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;

namespace FuturesTestTask.MarketDataService.Infrastructure.Common;

public abstract class MarketDataServiceFactory
{
    public abstract IMarketDataService CreateService();
}