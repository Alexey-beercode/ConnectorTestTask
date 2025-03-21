using System.Net;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Infrastructure.Configuration;
using FuturesTestTask.MarketDataService.Infrastructure.Persistence.Database;
using FuturesTestTask.MarketDataService.Infrastructure.Services.Binance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace FuturesTestTask.MarketDataService.Tests.TestUtils;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly FuturesDbContext DbContext;

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<FuturesDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        
        services.Configure<BybitOptions>(opts =>
        {
            opts.FuturesApiBaseUrl = "https://testnet.binance.com/api"; 
        });
        
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var json = JsonConvert.SerializeObject(new
                {
                    retCode = 0,
                    retMsg = "OK",
                    result = new
                    {
                        symbol = "BTCUSDT",
                        category = "linear",
                        list = new List<List<string>>
                        {
                            new List<string>
                            {
                                "1710806400000", // open time
                                "67643.9",       // open
                                "68174.9",       // high
                                "61555",         // low
                                "61971.5",       // close
                                "270938.291",    // volume
                                "17413254152.0052" // turnover
                            }
                        }
                    },
                    retExtInfo = new { },
                    time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                };
            });



        var httpClient = new HttpClient(httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://mocked-binance.api")
        };
        
        services.AddSingleton<IMarketDataService>(
            new BybitMarketDataService(httpClient, Options.Create(new BybitOptions
            {
                FuturesApiBaseUrl = "https://mocked-binance.api"
            })));
        
        MarketDataService = new BybitMarketDataService(httpClient, Options.Create(new BybitOptions
        {
            FuturesApiBaseUrl = "https://mocked-binance.api"
        }));


        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<FuturesDbContext>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => DbContext.Database.EnsureDeletedAsync();
    public IMarketDataService MarketDataService { get; }

}