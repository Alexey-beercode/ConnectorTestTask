using ConnectorTestTask.Core.Clients;
using ConnectorTestTask.Core.Implementations;
using ConnectorTestTask.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using System;
using System.Net.Http;
using Polly;

namespace ConnectorTestTask.Presentation.Extensions
{
    public static class WebApplicationBuilderExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddHttpClient<IBinanceRestClient, BinanceRestClient>()
                .AddResilienceHandler("binance-api", builder =>
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

            builder.Services.AddSingleton<IBinanceWebSocketClient, BinanceWebSocketClient>();
            builder.Services.AddSingleton<ITestConnector, BinanceConnector>();
            builder.Services.AddControllersWithViews();
        }
    }
}