FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
ENV DOTNET_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EventsIngestion.Service/EventsIngestion.Service.csproj", "EventsIngestion.Service/"]
RUN dotnet restore "EventsIngestion.Service/EventsIngestion.Service.csproj"
COPY . .
RUN dotnet publish "EventsIngestion.Service/EventsIngestion.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "EventsIngestion.Service.dll"]
