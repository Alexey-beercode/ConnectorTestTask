using FuturesTestTask.MarketDataService.Domain.Common;
using FuturesTestTask.MarketDataService.Domain.Entities;

namespace FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;

public interface IFuturesDataRepository : IBaseRepository<FuturesData>
{
    Task<FuturesData?> GetByDateAsync(DateTime date, CancellationToken cancellationToken=default);
    Task<PaginatedResult<FuturesData>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}