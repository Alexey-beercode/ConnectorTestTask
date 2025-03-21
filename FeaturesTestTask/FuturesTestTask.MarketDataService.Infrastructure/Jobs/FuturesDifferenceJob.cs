using System;
using System.Threading.Tasks;
using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.CreateFuturesDifference;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FuturesTestTask.MarketDataService.Infrastructure.Jobs;

public class FuturesDifferenceJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<FuturesDifferenceJob> _logger;

    public FuturesDifferenceJob(IMediator mediator, ILogger<FuturesDifferenceJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("📈 Старт задачи расчета фьючерсной разницы: {Time}", DateTime.UtcNow);

        var command = new CreateFuturesDifferenceCommand
        {
            Date = DateTime.UtcNow.Date,
            Interval = "1d"
        };

        await _mediator.Send(command);

        _logger.LogInformation("✅ Задача расчета фьючерсной разницы завершена: {Time}", DateTime.UtcNow);
    }
}