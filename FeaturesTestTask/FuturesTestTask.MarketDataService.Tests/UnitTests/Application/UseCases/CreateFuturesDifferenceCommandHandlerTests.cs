using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.CreateFuturesDifference;
using FeaturesTestTask.MarketDataService.Domain.Interfaces.Factories;
using FuturesTestTask.MarketDataService.Domain.Entities;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Services;
using FuturesTestTask.MarketDataService.Domain.Interfaces.UnitOfWork;
using Moq;

namespace FuturesTestTask.MarketDataService.Tests.UnitTests.Application.UseCases;

public class CreateFuturesDifferenceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidPrices_SavesFuturesData()
    {
        // Arrange
        var marketServiceMock = new Mock<IMarketDataService>();
        marketServiceMock.Setup(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), "1d", It.IsAny<DateTime>()))
            .ReturnsAsync(1000m);
        var futuresRepoMock = new Mock<IFuturesDataRepository>();
        futuresRepoMock.Setup(r => r.CreateAsync(It.IsAny<FuturesData>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.FuturesData).Returns(futuresRepoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);


        var factoryMock = new Mock<IMarketDataServiceFactory>();
        factoryMock.Setup(f => f.CreateService())
            .Returns(marketServiceMock.Object);

        var repoMock = new Mock<IUnitOfWork>();
        repoMock.Setup(u => u.FuturesData.CreateAsync(It.IsAny<FuturesData>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repoMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateFuturesDifferenceCommandHandler(factoryMock.Object, repoMock.Object);
        var command = new CreateFuturesDifferenceCommand
        {
            Interval = "1d",
            Date = DateTime.UtcNow.Date
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        repoMock.Verify(r => r.FuturesData.CreateAsync(It.IsAny<FuturesData>(), It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_FallbackToPreviousDay_ReturnsCorrectPrice()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        int callCount = 0;

        var marketServiceMock = new Mock<IMarketDataService>();
        marketServiceMock
            .Setup(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), "1d", It.IsAny<DateTime>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount % 2 == 0 ? 950m : (decimal?)null;
            });

        var factoryMock = new Mock<IMarketDataServiceFactory>();
        factoryMock.Setup(f => f.CreateService())
            .Returns(marketServiceMock.Object);

        var futuresRepoMock = new Mock<IFuturesDataRepository>();
        futuresRepoMock.Setup(r => r.CreateAsync(It.IsAny<FuturesData>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock.Setup(u => u.FuturesData).Returns(futuresRepoMock.Object);
        unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateFuturesDifferenceCommandHandler(factoryMock.Object, unitOfWorkMock.Object);

        var command = new CreateFuturesDifferenceCommand
        {
            Interval = "1d",
            Date = today
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        marketServiceMock.Verify(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), "1d", today), Times.AtLeastOnce);
        marketServiceMock.Verify(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), "1d", today.AddDays(-1)), Times.AtLeastOnce);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    
    [Fact]
    public async Task Handle_ThrowsException_WhenPriceNotFoundAfterRetries()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;

        var marketServiceMock = new Mock<IMarketDataService>();
        marketServiceMock.Setup(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), "1d", It.IsAny<DateTime>()))
            .ReturnsAsync((decimal?)null); // Всегда null

        var factoryMock = new Mock<IMarketDataServiceFactory>();
        factoryMock.Setup(f => f.CreateService())
            .Returns(marketServiceMock.Object);

        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateFuturesDifferenceCommandHandler(factoryMock.Object, unitOfWorkMock.Object);

        var command = new CreateFuturesDifferenceCommand
        {
            Interval = "1d",
            Date = today
        };

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));

        // Assert: Called exactly 10 раз
        marketServiceMock.Verify(s => s.GetFuturesClosePriceAsync(It.IsAny<string>(), "1d", It.IsAny<DateTime>()), Times.Exactly(10));
    }


}