using FuturesTestTask.MarketDataService.Domain.Entities;
using FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database.Configurations;
using Microsoft.EntityFrameworkCore;

namespace FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database;

public class FuturesDbContext : DbContext
{
    public FuturesDbContext(DbContextOptions<FuturesDbContext> options) : base(options)
    { }
    
    public DbSet<FuturesData> FuturesData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new FuturesDataConfiguration());
    }
}