using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.GetFuturesDifferences;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FuturesTestTask.MarketDataService.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuturesDataController : ControllerBase
{
    private readonly IMediator _mediator;

    public FuturesDataController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetFuturesDifferencesQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}