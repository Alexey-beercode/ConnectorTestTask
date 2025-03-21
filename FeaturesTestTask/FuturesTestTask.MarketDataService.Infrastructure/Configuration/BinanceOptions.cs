namespace FuturesTestTask.MarketDataService.Infrastructure.Configuration;

public class BinanceOptions
{
    public const string SectionName = "Binance";
    public string FuturesApiBaseUrl { get; set; } = default!;
}