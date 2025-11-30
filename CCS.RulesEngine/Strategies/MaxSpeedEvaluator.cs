using CCS.RulesEngine.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;

namespace CCS.RulesEngine.Strategies;

public class MaxSpeedEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.MaxSpeed;

    public bool Evaluate(TelemetryReceived telemetry, string threshold, out string violationMessage)
    {
        violationMessage = string.Empty;

        if (double.TryParse(threshold, out double limit))
        {
            if (telemetry.Speed > limit)
            {
                violationMessage = $"Velocidad {telemetry.Speed} km/h supera el límite de {limit} km/h";
                return true;
            }
        }
        return false;
    }
}