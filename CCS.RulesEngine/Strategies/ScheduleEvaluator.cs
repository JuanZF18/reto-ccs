using CCS.RulesEngine.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;

namespace CCS.RulesEngine.Strategies;

public class ScheduleEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.RestrictedSchedule;

    public bool Evaluate(TelemetryReceived telemetry, string threshold, out string violationMessage)
    {
        violationMessage = string.Empty;

        if (telemetry.Speed <= 0) return false;

        var parts = threshold.Split('-');
        if (parts.Length != 2) return false;

        if (TimeSpan.TryParse(parts[0], out var start) && TimeSpan.TryParse(parts[1], out var end))
        {
            var localTime = telemetry.Timestamp.AddHours(-5);
            var now = localTime.TimeOfDay;
            bool isInForbiddenRange;

            Console.WriteLine($"[DEBUG SCHEDULE] Vehículo: {telemetry.VehiclePlate} | Hora Local: {now:hh\\:mm} | Regla: {threshold}");

            // Caso A: Rango en el mismo día (Ej: 14:00 a 16:00)
            if (start <= end)
            {
                isInForbiddenRange = now >= start && now <= end;
            }
            // Caso B: Rango cruza la medianoche (Ej: 23:00 a 05:00)
            else
            {
                isInForbiddenRange = now >= start || now <= end;
            }

            if (isInForbiddenRange)
            {
                violationMessage = $"Movimiento no autorizado en horario restringido ({threshold}).";
                return true;
            }
        }
        return false;
    }
}