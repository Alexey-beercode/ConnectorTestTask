using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Domain.Interfaces.UnitOfWork;
using MediatR;


namespace FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.CreateFuturesDifference;

public class CreateFuturesDifferenceCommandHandler : IRequestHandler<CreateFuturesDifferenceCommand>
    {
        private readonly IMarketDataService _marketDataService;
        private readonly IUnitOfWork _unitOfWork;

        private const string QuarterSymbol = "BTCUSD_QUARTER";
        private const string BiQuarterSymbol = "BTCUSD_NEXT_QUARTER";

        public CreateFuturesDifferenceCommandHandler(
            IMarketDataService marketDataService,
            IUnitOfWork unitOfWork)
        {
            _marketDataService = marketDataService;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CreateFuturesDifferenceCommand request, CancellationToken cancellationToken)
        {
            var quarterPrice = await GetValidPriceWithFallback(QuarterSymbol, request.Interval, request.Date, cancellationToken);
            var biQuarterPrice = await GetValidPriceWithFallback(BiQuarterSymbol, request.Interval, request.Date, cancellationToken);

            var futuresData = new FuturesTestTask.MarketDataService.Domain.Entities.FuturesData(quarterPrice, biQuarterPrice, request.Date);

            await _unitOfWork.FuturesData.CreateAsync(futuresData, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<decimal> GetValidPriceWithFallback(string symbol, string interval, DateTime fromDate, CancellationToken ct)
        {
            var date = fromDate;

            for (int i = 0; i < 10; i++) 
            {
                var price = await _marketDataService.GetFuturesClosePriceAsync(symbol, interval, date);
                if (price.HasValue)
                    return price.Value;

                date = date.AddDays(-1); 
            }

            throw new Exception($"Не удалось получить цену для {symbol} начиная с {fromDate:yyyy-MM-dd}");
        }
    }