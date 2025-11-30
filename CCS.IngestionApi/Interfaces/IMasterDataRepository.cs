using CCS.IngestionApi.DTOs;

namespace CCS.IngestionApi.Interfaces;

/// <summary>
/// Contrato para el repositorio de datos maestros.
/// </summary>
public interface IMasterDataRepository
{
    /// <summary>Crea un nuevo propietario.</summary>
    Task<int> CreateOwnerAsync(CreateOwnerRequest req);

    /// <summary>Registra un nuevo vehículo.</summary>
    Task CreateVehicleAsync(CreateVehicleRequest req);

    /// <summary>Configura una regla para un vehículo.</summary>
    Task CreateRuleAsync(string plate, CreateRuleRequest req);

    /// <summary>Verifica si un propietario existe.</summary>
    Task<bool> OwnerExistsAsync(int id);
}