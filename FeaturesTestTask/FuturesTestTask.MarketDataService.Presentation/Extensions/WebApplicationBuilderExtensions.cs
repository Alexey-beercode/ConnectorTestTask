using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.CreateFuturesDifference;
using FeaturesTestTask.MarketDataService.Domain.Interfaces.Factories;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Domain.Interfaces.UnitOfWork;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Jobs;
using FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database;
using FuturesTestTask.MarketDataService.Infrastructure.Repositories;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Decorators;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Factories;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;


namespace FuturesTestTask.MarketDataService.Infrastructure.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.RegisterDbContext();
        builder.RegisterBinance();
        builder.RegisterRepositories();
        builder.RegisterUnitOfWork();
        builder.RegisterJobs();
        builder.RegisterUseCases();
        builder.AddBinanceHttpResiliencePolicy();
        builder.ConfigureSerilog();
        return builder;
    }

    public static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
    private static void RegisterDbContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string is missing");

        builder.Services.AddDbContext<FuturesDbContext>(options =>
            options.UseNpgsql(connectionString));
    }
    private static void AddBinanceHttpResiliencePolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("BybitClient")
            .AddResilienceHandler("bybit-api", builder =>
            {
                builder.AddRetry(new()
                {
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(2)
                });

                builder.AddTimeout(TimeSpan.FromSeconds(10));

                builder.AddCircuitBreaker(new()
                {
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(15),
                    MinimumThroughput = 5
                });
            });

    }

    private static void RegisterBinance(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<BybitOptions>(
            builder.Configuration.GetSection(BybitOptions.SectionName));

        builder.Services.AddHttpClient();

        builder.Services.AddScoped<IMarketDataServiceFactory, BybitMarketDataServiceFactory>();
        
        builder.Services.AddScoped<IMarketDataService>(sp =>
        {
            var factory = sp.GetRequiredService<IMarketDataServiceFactory>();
            var logger = sp.GetRequiredService<ILogger<LoggingMarketDataService>>();
            return new LoggingMarketDataService(factory.CreateService(), logger);
        });
    }

    private static void RegisterRepositories(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IFuturesDataRepository, FuturesDataRepository>();
    }

    private static void RegisterUnitOfWork(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
    }

    private static void RegisterJobs(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<FuturesDifferenceJob>();
    }

    private static void RegisterUseCases(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<CreateFuturesDifferenceCommandHandler>());
    }
}
