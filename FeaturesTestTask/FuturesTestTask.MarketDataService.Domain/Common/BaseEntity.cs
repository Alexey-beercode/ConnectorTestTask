using FuturesTestTask.MarketDataService.Domain.Interfaces.Entities;

namespace FeaturesTestTask.MarketDataService.Domain.Common;

public class BaseEntity : IHasId,ISoftDeletable
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
}