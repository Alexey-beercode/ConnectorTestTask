using FuturesTestTask.MarketDataService.Domain.Common;
using FuturesTestTask.MarketDataService.Domain.Entities;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;
using FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace FuturesTestTask.MarketDataService.Infrastructure.Repositories;

public class FuturesDataRepository : BaseRepository<FuturesData>, IFuturesDataRepository
{
    public FuturesDataRepository(FuturesDbContext dbContext) : base(dbContext)
    { }

    public async Task<FuturesData?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(data => data.Timestamp == date, cancellationToken );
    }
    
    public async Task<PaginatedResult<FuturesData>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Timestamp);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<FuturesData>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

}