using MediatR;

namespace FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.CreateFuturesDifference;

public class CreateFuturesDifferenceCommand : IRequest
{
    public DateTime Date { get; init; }
    public string Interval { get; init; } = "1d"; // default
}