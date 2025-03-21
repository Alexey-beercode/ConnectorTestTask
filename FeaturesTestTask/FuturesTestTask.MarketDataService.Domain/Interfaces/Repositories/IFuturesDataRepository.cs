using FuturesTestTask.MarketDataService.Domain.Entities;

namespace FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;

public interface IFuturesDataRepository : IBaseRepository<FuturesData>
{
    Task<FuturesData?> GetByDateAsync(DateTime date, CancellationToken cancellationToken=default);
}