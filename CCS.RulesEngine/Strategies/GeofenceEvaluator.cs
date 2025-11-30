using CCS.RulesEngine.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;
using System.Globalization;

namespace CCS.RulesEngine.Strategies;

public class GeofenceEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.Geofence;

    public bool Evaluate(TelemetryReceived telemetry, string thresholdValue, out string violationMessage)
    {
        violationMessage = string.Empty;

        var parts = thresholdValue.Split(',');
        if (parts.Length != 3) return false;

        if (!double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double centerLat) ||
            !double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double centerLon) ||
            !double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double radiusMeters))
        {
            return false;
        }

        double distance = CalculateDistance(telemetry.Latitude, telemetry.Longitude, centerLat, centerLon);

        if (distance > radiusMeters)
        {
            violationMessage = $"Vehículo fuera de geocerca. Distancia al centro: {distance:F0}m (Radio permitido: {radiusMeters}m).";
            return true;
        }

        return false;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3; // Radio tierra (metros)
        var phi1 = lat1 * Math.PI / 180;
        var phi2 = lat2 * Math.PI / 180;
        var deltaPhi = (lat2 - lat1) * Math.PI / 180;
        var deltaLambda = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}