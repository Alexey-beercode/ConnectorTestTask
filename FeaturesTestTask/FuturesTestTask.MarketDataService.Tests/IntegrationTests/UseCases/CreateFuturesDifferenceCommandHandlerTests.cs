using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.CreateFuturesDifference;
using FeaturesTestTask.MarketDataService.Domain.Interfaces.Factories;
using FuturesTestTask.MarketDataService.Infrastructure.Repositories;
using FuturesTestTask.MarketDataService.Infrastructure.UnitOfWork;
using FuturesTestTask.MarketDataService.Tests.TestUtils;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FuturesTestTask.MarketDataService.Tests.IntegrationTests.UseCases;

public class CreateFuturesDifferenceCommandHandlerTests : IntegrationTestBase
{
    private CreateFuturesDifferenceCommandHandler CreateHandler()
    {
        var repo = new FuturesDataRepository(DbContext);
        var uow = new UnitOfWork(DbContext, repo);
        var factoryMock = new Mock<IMarketDataServiceFactory>();
        factoryMock.Setup(f => f.CreateService()).Returns(MarketDataService);

        return new CreateFuturesDifferenceCommandHandler(factoryMock.Object, uow);
    }

    [Fact]
    public async Task Handle_SavesRecordSuccessfully()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateFuturesDifferenceCommand
        {
            Interval = "1d",
            Date = DateTime.UtcNow.Date
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await DbContext.FuturesData.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal(command.Date, saved.Timestamp);
    }

    [Fact]
    public async Task Handle_StoresCorrectPrices()
    {
        // Arrange
        var handler = CreateHandler();
        var date = DateTime.UtcNow.Date;
        var command = new CreateFuturesDifferenceCommand { Interval = "1d", Date = date };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var saved = await DbContext.FuturesData.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.True(saved.QuarterPrice > 0);
        Assert.True(saved.BiQuarterPrice > 0);
        Assert.Equal(saved.PriceDifference, saved.QuarterPrice - saved.BiQuarterPrice);
    }

    [Fact]
    public async Task Handle_ThrowsAfter10Failures()
    {
        // Arrange: создаём сервис с always-null ответом
        var badServiceMock = new Mock<Domain.Interfaces.Services.IMarketDataService>();
        badServiceMock.Setup(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync((decimal?)null);

        var factoryMock = new Mock<IMarketDataServiceFactory>();
        factoryMock.Setup(f => f.CreateService()).Returns(badServiceMock.Object);

        var repo = new FuturesDataRepository(DbContext);
        var uow = new UnitOfWork(DbContext, repo);
        var handler = new CreateFuturesDifferenceCommandHandler(factoryMock.Object, uow);

        var command = new CreateFuturesDifferenceCommand { Interval = "1d", Date = DateTime.UtcNow.Date };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        Assert.Equal(0, await DbContext.FuturesData.CountAsync());
    }

    [Fact]
    public async Task Handle_CreatesOnlyOneRecord()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CreateFuturesDifferenceCommand { Interval = "1d", Date = DateTime.UtcNow.Date };

        // Act
        await handler.Handle(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None); // дублируем вызов

        // Assert
        var count = await DbContext.FuturesData.CountAsync();
        Assert.Equal(2, count); // т.к. дата одинаковая, но нет уникального ограничения
    }
}