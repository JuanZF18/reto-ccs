# CCS.IngestionApi/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# 1. Copiar los archivos de proyecto (csproj) para restaurar dependencias
COPY ["CCS.IngestionApi/CCS.IngestionApi.csproj", "CCS.IngestionApi/"]
COPY ["CCS.Shared/CCS.Shared.csproj", "CCS.Shared/"]
RUN dotnet restore "CCS.IngestionApi/CCS.IngestionApi.csproj"

# 2. Copiar el resto del código y compilar
COPY . .
WORKDIR "/src/CCS.IngestionApi"
RUN dotnet build "CCS.IngestionApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CCS.IngestionApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CCS.IngestionApi.dll"]