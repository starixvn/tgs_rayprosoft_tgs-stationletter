FROM hub.lumigame.com/base/dotnet/core/sdk:8.0 AS builder

ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.StationLetter/Tgs.StationLetter.csproj", "Tgs.StationLetter/"]
COPY ["StationLetter.Core/StationLetter.Core.csproj", "StationLetter.Core/"]
COPY ["StationLetter.Repositories/StationLetter.Repositories.csproj", "StationLetter.Repositories/"]
RUN dotnet restore "Tgs.StationLetter/Tgs.StationLetter.csproj"
COPY . .
WORKDIR "/src/Tgs.StationLetter"
RUN dotnet publish "Tgs.StationLetter.csproj" -c Release -o /app /p:UseAppHost=false

FROM hub.lumigame.com/base/dotnet/core/aspnet:8.0-alpine-24h1.1 AS base
WORKDIR /app
COPY --from=builder /app .

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Tgs.StationLetter.dll"]
