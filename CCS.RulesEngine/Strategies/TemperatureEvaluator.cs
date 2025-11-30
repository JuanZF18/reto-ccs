using CCS.RulesEngine.Interfaces;
using CCS.Shared.Constants;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;

namespace CCS.RulesEngine.Strategies;

public class TemperatureEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.CargoTemperature;

    public bool Evaluate(TelemetryReceived telemetry, string threshold, out string violationMessage)
    {
        violationMessage = string.Empty;

        // Buscamos la temperatura en la metadata
        if (telemetry.Metadata != null &&
            telemetry.Metadata.TryGetValue(TelemetryKeys.CargoTemperature, out var tempObj))
        {
            // El threshold viene de BD ej: "-5:10" (Min -5, Max 10) o simplemente "Max"
            // Para simplificar, asumiremos que threshold es la Temperatura MÁXIMA permitida.

            if (double.TryParse(tempObj.ToString(), out double currentTemp) &&
                double.TryParse(threshold, out double maxTemp))
            {
                if (currentTemp > maxTemp)
                {
                    violationMessage = $"Ruptura de Cadena de Frío. Temp Actual: {currentTemp}°C > Máx: {maxTemp}°C";
                    return true;
                }
            }
        }
        return false;
    }
}