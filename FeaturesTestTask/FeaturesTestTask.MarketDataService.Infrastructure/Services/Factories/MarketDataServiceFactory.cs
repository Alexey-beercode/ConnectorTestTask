using FeaturesTestTask.MarketDataService.Domain.Interfaces.Services;

namespace FeaturesTestTask.MarketDataService.Infrastructure.Services.Factories;

public abstract class MarketDataServiceFactory
{
    public abstract IMarketDataService CreateService();
}