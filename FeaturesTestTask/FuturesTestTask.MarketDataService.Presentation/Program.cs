using FuturesTestTask.MarketDataService.Infrastructure.Extensions;
using FuturesTestTask.MarketDataService.Presentation.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.RegisterServices();

var app = builder.Build();

app.ConfigureMiddleware();
app.Run();