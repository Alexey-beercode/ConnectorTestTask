using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.GetFuturesDifferences;
using FuturesTestTask.MarketDataService.Domain.Entities;
using FuturesTestTask.MarketDataService.Infrastructure.Repositories;
using FuturesTestTask.MarketDataService.Tests.TestUtils;

namespace FuturesTestTask.MarketDataService.Tests.IntegrationTests.UseCases;

public class GetFuturesDifferencesQueryHandlerTests : IntegrationTestBase
{
    private async Task SeedDataAsync()
    {
        var entries = new List<FuturesData>
        {
            new(1000, 950, DateTime.UtcNow.AddDays(-3)),
            new(1050, 980, DateTime.UtcNow.AddDays(-2)),
            new(1100, 1000, DateTime.UtcNow.AddDays(-1))
        };

        await DbContext.FuturesData.AddRangeAsync(entries);
        await DbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_ReturnsAllItems_WhenPageSizeLarge()
    {
        // Arrange
        await SeedDataAsync();
        var repository = new FuturesDataRepository(DbContext);
        var handler = new GetFuturesDifferencesQueryHandler(repository);
        var query = new GetFuturesDifferencesQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult_WhenPageSizeIsOne()
    {
        // Arrange
        await SeedDataAsync();
        var repository = new FuturesDataRepository(DbContext);
        var handler = new GetFuturesDifferencesQueryHandler(repository);
        var query = new GetFuturesDifferencesQuery { Page = 1, PageSize = 1 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Handle_ReturnsEmpty_WhenNoRecordsExist()
    {
        // Arrange
        var repository = new FuturesDataRepository(DbContext);
        var handler = new GetFuturesDifferencesQueryHandler(repository);
        var query = new GetFuturesDifferencesQuery { Page = 1, PageSize = 10 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPageData()
    {
        // Arrange
        await SeedDataAsync();
        var repository = new FuturesDataRepository(DbContext);
        var handler = new GetFuturesDifferencesQueryHandler(repository);
        var query = new GetFuturesDifferencesQuery { Page = 2, PageSize = 2 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(3, result.TotalCount);
    }
}