using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;

namespace FuturesTestTask.MarketDataService.Domain.Interfaces.UnitOfWork;

public interface IUnitOfWork:IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
    IFuturesDataRepository FuturesData { get; }
}