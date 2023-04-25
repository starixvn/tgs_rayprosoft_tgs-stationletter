#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BuildConfiguration=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.StationLetter/Tgs.StationLetter.csproj", "Tgs.StationLetter/"]
COPY ["StationLetter.Core/StationLetter.Core.csproj", "StationLetter.Core/"]
COPY ["StationLetter.Repositories/StationLetter.Repositories.csproj", "StationLetter.Repositories/"]
RUN dotnet restore "Tgs.StationLetter/Tgs.StationLetter.csproj"
COPY . .
WORKDIR "/src/Tgs.StationLetter"
RUN dotnet build "Tgs.StationLetter.csproj" -c $BuildConfiguration -o /app/build

FROM build AS publish
ARG BuildConfiguration
RUN dotnet publish "Tgs.StationLetter.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.StationLetter.dll"]