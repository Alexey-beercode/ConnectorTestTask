namespace FuturesTestTask.MarketDataService.Infrastructure.Configuration;

public class BybitOptions
{
    public const string SectionName = "Bybit";
    public string FuturesApiBaseUrl { get; set; } = default!;
}