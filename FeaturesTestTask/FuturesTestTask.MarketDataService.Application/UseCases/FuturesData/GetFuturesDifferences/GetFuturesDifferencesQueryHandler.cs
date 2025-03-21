using FuturesTestTask.MarketDataService.Domain.Common;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;
using MediatR;

namespace FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.GetFuturesDifferences;

public class GetFuturesDifferencesQueryHandler : IRequestHandler<GetFuturesDifferencesQuery, PaginatedResult<FuturesTestTask.MarketDataService.Domain.Entities.FuturesData>>
{
    private readonly IFuturesDataRepository _repository;

    public GetFuturesDifferencesQueryHandler(IFuturesDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<FuturesTestTask.MarketDataService.Domain.Entities.FuturesData>> Handle(GetFuturesDifferencesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
    }
}