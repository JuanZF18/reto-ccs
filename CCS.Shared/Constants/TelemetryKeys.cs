namespace CCS.Shared.Constants;

public static class TelemetryKeys
{
    // Sensores de Carga
    public const string CargoTemperature = "cargo_temp"; // double
    public const string CargoDoorStatus = "cargo_door";  // string: "OPEN", "CLOSED"
    public const string CargoVibration = "cargo_vib";    // double (para accidentes/golpes)

    // Seguridad
    public const string PanicButton = "panic_btn";       // boolean
    public const string MechanicHelp = "mechanic_help";  // boolean
    public const string SecurityHelp = "security_help";  // boolean

    // Video
    public const string CameraStatus = "cam_status";     // string: "REC", "IDLE", "ERR"
    public const string VideoEventUrl = "vid_url";       // string (URL del clip del evento)
}