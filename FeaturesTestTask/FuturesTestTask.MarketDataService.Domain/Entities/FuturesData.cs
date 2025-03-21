using System.ComponentModel.DataAnnotations.Schema;
using FeaturesTestTask.MarketDataService.Domain.Common;

namespace FuturesTestTask.MarketDataService.Domain.Entities;

[Table("futures-data")]
public class FuturesData : BaseEntity
{
    public DateTime Timestamp { get; private set; }
    public decimal QuarterPrice { get; private set; }
    public decimal BiQuarterPrice { get; private set; }
    public decimal PriceDifference { get; private set; } 
    
    public FuturesData(decimal quarterPrice, decimal biQuarterPrice, DateTime timestamp)
    {
        Id = Guid.NewGuid();
        Timestamp = timestamp;
        QuarterPrice = quarterPrice;
        BiQuarterPrice = biQuarterPrice;
        PriceDifference = quarterPrice - biQuarterPrice;
    }

}
