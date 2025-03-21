using Moq;
using FeaturesTestTask.MarketDataService.Application.UseCases.FuturesData.GetFuturesDifferences;
using FuturesTestTask.MarketDataService.Domain.Common;
using FuturesTestTask.MarketDataService.Domain.Entities;
using FuturesTestTask.MarketDataService.Domain.Interfaces.Repositories;

public class GetFuturesDifferencesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPaginatedResult()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        var mockData = new List<FuturesData>
        {
            new(1000, 950, DateTime.UtcNow)
        };

        var expectedResult = new PaginatedResult<FuturesData>
        {
            Items = mockData,
            TotalCount = mockData.Count,
        };

        var repositoryMock = new Mock<IFuturesDataRepository>();
        repositoryMock
            .Setup(r => r.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var handler = new GetFuturesDifferencesQueryHandler(repositoryMock.Object);

        var query = new GetFuturesDifferencesQuery
        {
            Page = page,
            PageSize = pageSize
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        repositoryMock.Verify(r => r.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ReturnsEmptyResult_WhenNoData()
    {
        // Arrange
        var page = 1;
        var pageSize = 10;

        var emptyResult = new PaginatedResult<FuturesData>
        {
            Items = new List<FuturesData>(),
            TotalCount = 0,
        };

        var repositoryMock = new Mock<IFuturesDataRepository>();
        repositoryMock
            .Setup(r => r.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        var handler = new GetFuturesDifferencesQueryHandler(repositoryMock.Object);

        var query = new GetFuturesDifferencesQuery
        {
            Page = page,
            PageSize = pageSize
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        repositoryMock.Verify(r => r.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ReturnsSingleItem_WhenPageSizeIsOne()
    {
        // Arrange
        var page = 1;
        var pageSize = 1;

        var testData = new List<FuturesData>
        {
            new(1020, 990, DateTime.UtcNow)
        };

        var pagedResult = new PaginatedResult<FuturesData>
        {
            Items = testData,
            TotalCount = testData.Count,
        };

        var repositoryMock = new Mock<IFuturesDataRepository>();
        repositoryMock
            .Setup(r => r.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var handler = new GetFuturesDifferencesQueryHandler(repositoryMock.Object);

        var query = new GetFuturesDifferencesQuery
        {
            Page = page,
            PageSize = pageSize
        };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        repositoryMock.Verify(r => r.GetPagedAsync(page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }
}