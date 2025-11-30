# CCS.RulesEngine/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CCS.RulesEngine/CCS.RulesEngine.csproj", "CCS.RulesEngine/"]
COPY ["CCS.Shared/CCS.Shared.csproj", "CCS.Shared/"]
RUN dotnet restore "CCS.RulesEngine/CCS.RulesEngine.csproj"

COPY . .
WORKDIR "/src/CCS.RulesEngine"
RUN dotnet build "CCS.RulesEngine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CCS.RulesEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CCS.RulesEngine.dll"]