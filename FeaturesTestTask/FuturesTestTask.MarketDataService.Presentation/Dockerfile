FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FuturesTestTask.MarketDataService.Presentation/FuturesTestTask.MarketDataService.Presentation.csproj", "FuturesTestTask.MarketDataService.Presentation/"]
COPY ["FuturesTestTask.MarketDataService.Application/FuturesTestTask.MarketDataService.Application.csproj", "FuturesTestTask.MarketDataService.Application/"]
COPY ["FuturesTestTask.MarketDataService.Domain/FuturesTestTask.MarketDataService.Domain.csproj", "FuturesTestTask.MarketDataService.Domain/"]
COPY ["FuturesTestTask.MarketDataService.Infrastructure/FuturesTestTask.MarketDataService.Infrastructure.csproj", "FuturesTestTask.MarketDataService.Infrastructure/"]
RUN dotnet restore "FuturesTestTask.MarketDataService.Presentation/FuturesTestTask.MarketDataService.Presentation.csproj"
COPY . .
WORKDIR "/src/FuturesTestTask.MarketDataService.Presentation"
RUN dotnet build "FuturesTestTask.MarketDataService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FuturesTestTask.MarketDataService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FuturesTestTask.MarketDataService.Presentation.dll"]
