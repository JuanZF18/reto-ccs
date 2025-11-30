# CCS.NotificationService/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CCS.NotificationService/CCS.NotificationService.csproj", "CCS.NotificationService/"]
COPY ["CCS.Shared/CCS.Shared.csproj", "CCS.Shared/"]
RUN dotnet restore "CCS.NotificationService/CCS.NotificationService.csproj"

COPY . .
WORKDIR "/src/CCS.NotificationService"
RUN dotnet build "CCS.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CCS.NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CCS.NotificationService.dll"]