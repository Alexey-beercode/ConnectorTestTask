namespace FuturesTestTask.MarketDataService.Infrastructure.Models;

public class BybitKlineResult
{
    public string Symbol { get; set; }
    public string Category { get; set; }
    public List<List<string>> List { get; set; }
}