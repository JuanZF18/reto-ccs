using CCS.RulesEngine.Interfaces;
using CCS.Shared.Constants;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;
using System.Globalization;

namespace CCS.RulesEngine.Strategies;

public class PanicEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.PanicButton;

    public bool Evaluate(TelemetryReceived telemetry, string threshold, out string violationMessage)
    {
        violationMessage = string.Empty;
        var meta = telemetry.Metadata;

        if (meta == null) return false;

        // ---------------------------------------------------------
        // 0. Preparar Evidencia (Video) si existe
        // ---------------------------------------------------------
        string videoEvidence = "";
        if (meta.TryGetValue(TelemetryKeys.VideoEventUrl, out var urlObj))
        {
            videoEvidence = $" [EVIDENCIA VIDEO: {urlObj}]";
        }

        // ---------------------------------------------------------
        // 1. Botón de Pánico (Prioridad Alta)
        // ---------------------------------------------------------
        if (meta.TryGetValue(TelemetryKeys.PanicButton, out var panic) && IsTrue(panic))
        {
            violationMessage = $"¡ALERTA DE SEGURIDAD! Botón de Pánico Activado.{videoEvidence}";
            return true;
        }

        // ---------------------------------------------------------
        // 2. Ayuda Mecánica (Solicitud desde App)
        // ---------------------------------------------------------
        if (meta.TryGetValue(TelemetryKeys.MechanicHelp, out var mechanic) && IsTrue(mechanic))
        {
            violationMessage = $"Solicitud de Ayuda Mecánica recibida.{videoEvidence}";
            return true;
        }

        // ---------------------------------------------------------
        // 3. Ayuda de Seguridad
        // ---------------------------------------------------------
        if (meta.TryGetValue(TelemetryKeys.SecurityHelp, out var security) && IsTrue(security))
        {
            violationMessage = $"Solicitud de Apoyo de Seguridad recibida.{videoEvidence}";
            return true;
        }

        // ---------------------------------------------------------
        // 4. Accidente Detectado
        // ---------------------------------------------------------
        if (meta.TryGetValue(TelemetryKeys.CargoVibration, out var vib))
        {
            // Usamos InvariantCulture para parsear (por si viene texto)
            if (double.TryParse(Convert.ToString(vib, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out double gForce) && gForce > 10.0)
            {
                violationMessage = $"Posible Accidente detectado: Fuerza G {gForce.ToString(CultureInfo.InvariantCulture)}.{videoEvidence}";
                return true;
            }
        }

        // ---------------------------------------------------------
        // 5. Falla de Cámara
        // ---------------------------------------------------------
        if (meta.TryGetValue(TelemetryKeys.CameraStatus, out var status) && status.ToString() == "ERROR")
        {
            violationMessage = "Falla crítica en sistema de cámaras.";
            return true;
        }

        return false;
    }

    private bool IsTrue(object val) => val?.ToString()?.ToLower() == "true";
}