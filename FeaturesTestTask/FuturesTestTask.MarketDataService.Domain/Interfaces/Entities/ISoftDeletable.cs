namespace FuturesTestTask.MarketDataService.Domain.Interfaces.Entities;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
