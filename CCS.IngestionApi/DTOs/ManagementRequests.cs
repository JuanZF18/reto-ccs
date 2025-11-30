using CCS.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace CCS.IngestionApi.DTOs;

/// <summary>
/// Datos para registrar un nuevo propietario.
/// </summary>
public record CreateOwnerRequest(
    // <summary>Nombre completo del propietario o empresa.</summary>
    [Required] string Name,

    // <summary>Correo electrónico de contacto (usado para notificaciones).</summary>
    [Required, EmailAddress] string Email
);

/// <summary>
/// Datos para registrar un nuevo vehículo.
/// </summary>
public record CreateVehicleRequest(
    // <summary>Placa única del vehículo (Ej: AAA-123).</summary>
    [Required] string Plate,

    // <summary>Tipo de vehículo (1=Carro, 2=Moto, 3=Camión).</summary>
    [Required] VehicleType Type,

    // <summary>ID del propietario registrado previamente.</summary>
    [Required] int OwnerId
);

/// <summary>
/// Configuración de una regla de negocio para un vehículo.
/// </summary>
public record CreateRuleRequest(
    // <summary>Tipo de regla (1=Velocidad, 3=Parada, 4=Pánico, 5=Temp).</summary>
    [Required] RuleType RuleType,

    // <summary>Valor límite. Ej: "80" (km/h), "-5" (grados), "TRUE" (pánico).</summary>
    [Required] string Threshold
);

/// <summary>
/// Modelo de datos enviado por los dispositivos GPS o App Móvil.
/// </summary>
public record TelemetryRequest(
    // <summary>Placa única del vehículo (Ej: T-123)</summary>
    string Plate,

    // <summary>Velocidad actual en km/h</summary>
    double Speed,

    // <summary>Latitud geográfica (WGS84)</summary>
    double Lat,

    // <summary>Longitud geográfica (WGS84)</summary>
    double Lon,

    // <summary>Dirección de movimiento en grados (0-360)</summary>
    double Heading,

    // <summary>
    // Datos flexibles de sensores.
    // Claves soportadas: "cargo_temp" (double), "panic_btn" (bool), "door_status" (string).
    // </summary>
    Dictionary<string, object>? Metadata
);