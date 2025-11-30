namespace CCS.Shared.Contracts;

// Evento que entra al sistema (Ingestion -> RulesEngine)
public record TelemetryReceived(
    string VehiclePlate,
    double Latitude,
    double Longitude,
    double Speed,
    double Heading, // <--- NUEVO: Dirección (0-360 grados)
    DateTime Timestamp,
    Dictionary<string, object> Metadata // Aquí viaja Temp, Panic, DoorStatus, VideoUrl
);

// Evento de alerta (RulesEngine -> NotificationService)
public record AlertTriggered(
    string VehiclePlate,
    string RuleDescription, // Ej: "Exceso de velocidad > 80km/h"
    DateTime Timestamp
);