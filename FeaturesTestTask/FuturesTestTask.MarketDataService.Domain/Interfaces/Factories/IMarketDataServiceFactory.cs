using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;

namespace FeaturesTestTask.MarketDataService.Domain.Interfaces.Factories;

public interface IMarketDataServiceFactory
{
    IMarketDataService CreateService();
}
