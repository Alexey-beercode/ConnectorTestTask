using FuturesTestTask.MarketDataService.Infrastructure.Models;

namespace FuturesTestTask.MarketDataService.Infrastructure.Models;

public class BybitKlineResponse
{
    public int RetCode { get; set; }
    public string RetMsg { get; set; }
    public BybitKlineResult Result { get; set; }
    public object RetExtInfo { get; set; }
    public long Time { get; set; }
}