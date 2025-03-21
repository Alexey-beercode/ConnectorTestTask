using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;
using FuturesTestTask.MarketDataService.Domain.Interfaces.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace FuturesTestTask.MarketDataService.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private bool _disposed;
    private readonly DbContext _dbContext;
    private readonly IFuturesDataRepository _futuresDataRepository;

    public UnitOfWork(DbContext dbContext, IFuturesDataRepository futuresDataRepository)
    {
        _dbContext = dbContext;
        _futuresDataRepository = futuresDataRepository;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken=default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }

            _disposed = true;
        }
    }
    
    public IFuturesDataRepository FuturesData => _futuresDataRepository;
}