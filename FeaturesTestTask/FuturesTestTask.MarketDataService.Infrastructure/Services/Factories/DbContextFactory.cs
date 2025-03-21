using FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FuturesTestTask.MarketDataService.Infrastructure.Services.Factories
{
    public static class DbContextFactory
    {
        public static void AddCustomDbContextFactory(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddPooledDbContextFactory<FuturesDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
    }
}