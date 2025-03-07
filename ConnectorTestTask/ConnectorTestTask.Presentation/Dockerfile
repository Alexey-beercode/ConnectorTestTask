FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 7013

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ConnectorTestTask.Presentation/ConnectorTestTask.Presentation.csproj", "ConnectorTestTask.Presentation/"]
COPY ["ConnectorTestTask.Core/ConnectorTestTask.Core.csproj", "ConnectorTestTask.Core/"]
RUN dotnet restore "ConnectorTestTask.Presentation/ConnectorTestTask.Presentation.csproj"
COPY . .
WORKDIR "/src/ConnectorTestTask.Presentation"
RUN dotnet build "ConnectorTestTask.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ConnectorTestTask.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConnectorTestTask.Presentation.dll"]
