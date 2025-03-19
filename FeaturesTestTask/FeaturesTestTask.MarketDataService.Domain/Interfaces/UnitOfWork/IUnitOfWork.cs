using FeaturesTestTask.MarketDataService.Domain.Interfaces.Repositories;

namespace FeaturesTestTask.MarketDataService.Domain.Interfaces.UnitOfWork;

public interface IUnitOfWork:IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken=default);
    IFuturesDataRepository FuturesData { get; }
}