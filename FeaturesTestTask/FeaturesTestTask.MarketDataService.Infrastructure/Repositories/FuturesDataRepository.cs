using FeaturesTestTask.MarketDataService.Domain.Entities;
using FeaturesTestTask.MarketDataService.Domain.Interfaces.Repositories;
using FeaturesTestTask.MarketDataService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace FeaturesTestTask.MarketDataService.Infrastructure.Repositories;

public class FuturesDataRepository : BaseRepository<FuturesData>, IFuturesDataRepository
{
    public FuturesDataRepository(FuturesDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<FuturesData?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(data => data.Timestamp == date, cancellationToken );
    }
}