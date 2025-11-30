using CCS.IngestionApi.DTOs;
using CCS.IngestionApi.Interfaces;
using Dapper;
using Npgsql;

namespace CCS.IngestionApi.Data;

/// <summary>
/// Repositorio encargado de la gestión de datos maestros (Dueños, Vehículos y Reglas).
/// Utiliza Dapper para operaciones de alto rendimiento contra PostgreSQL.
/// </summary>
public class MasterDataRepository: IMasterDataRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Inicializa una nueva instancia del repositorio.
    /// </summary>
    /// <param name="config">Configuración de la aplicación para obtener la cadena de conexión.</param>
    /// <exception cref="ArgumentNullException">Se lanza si no se encuentra la cadena de conexión 'DefaultConnection'.</exception>
    public MasterDataRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string not found");
    }

    private NpgsqlConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    /// <summary>
    /// Crea un nuevo propietario en la base de datos.
    /// </summary>
    /// <param name="req">Objeto con los datos del propietario (Nombre, Email).</param>
    /// <returns>El ID único (PK) generado automáticamente para el nuevo propietario.</returns>
    public async Task<int> CreateOwnerAsync(CreateOwnerRequest req)
    {
        using var conn = CreateConnection();
        var sql = "INSERT INTO owners (name, email) VALUES (@Name, @Email) RETURNING id;";
        return await conn.QuerySingleAsync<int>(sql, req);
    }

    /// <summary>
    /// Registra un nuevo vehículo asociado a un propietario existente.
    /// </summary>
    /// <param name="req">Objeto con los datos del vehículo (Placa, Tipo, Dueño).</param>
    /// <remarks>El estado 'is_active' se establece en TRUE por defecto.</remarks>
    public async Task CreateVehicleAsync(CreateVehicleRequest req)
    {
        using var conn = CreateConnection();
        var sql = @"
            INSERT INTO vehicles (plate, type, owner_id, is_active) 
            VALUES (@Plate, (CAST(@Type AS int)), @OwnerId, true);";
        await conn.ExecuteAsync(sql, req);
    }

    /// <summary>
    /// Asigna una regla de monitoreo específica a un vehículo.
    /// </summary>
    /// <param name="plate">Placa del vehículo al que se le aplicará la regla.</param>
    /// <param name="req">Detalles de la regla (Tipo de regla y valor límite).</param>
    public async Task CreateRuleAsync(string plate, CreateRuleRequest req)
    {
        using var conn = CreateConnection();
        var sql = @"
            INSERT INTO rule_configs (vehicle_plate, rule_type, threshold_value, is_active) 
            VALUES (@Plate, (CAST(@RuleType AS int)), @Threshold, true);";

        await conn.ExecuteAsync(sql, new
        {
            Plate = plate,
            req.RuleType,
            req.Threshold
        });
    }

    /// <summary>
    /// Verifica si un propietario existe en la base de datos dado su ID.
    /// </summary>
    /// <param name="id">ID del propietario a verificar.</param>
    /// <returns>True si existe, False si no.</returns>
    public async Task<bool> OwnerExistsAsync(int id)
    {
        using var conn = CreateConnection();
        return await conn.ExecuteScalarAsync<bool>("SELECT EXISTS(SELECT 1 FROM owners WHERE id = @id)", new { id });
    }
}