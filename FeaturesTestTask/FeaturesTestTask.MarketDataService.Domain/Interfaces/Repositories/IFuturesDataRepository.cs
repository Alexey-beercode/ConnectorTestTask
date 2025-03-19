using FeaturesTestTask.MarketDataService.Domain.Entities;

namespace FeaturesTestTask.MarketDataService.Domain.Interfaces.Repositories;

public interface IFuturesDataRepository : IBaseRepository<FuturesData>
{
    Task<FuturesData?> GetByDateAsync(DateTime date, CancellationToken cancellationToken=default);
}