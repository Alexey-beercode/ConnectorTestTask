using FuturesTestTask.MarketDataService.Domain.Common;
using MediatR;

namespace FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.GetFuturesDifferences;

public class GetFuturesDifferencesQuery : IRequest<PaginatedResult<FuturesTestTask.MarketDataService.Domain.Entities.FuturesData>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}