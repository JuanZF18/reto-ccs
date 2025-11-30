using CCS.RulesEngine.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;

namespace CCS.RulesEngine.Strategies;

public class StopEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.UnplannedStop;

    public bool Evaluate(TelemetryReceived telemetry, string threshold, out string violationMessage)
    {
        violationMessage = string.Empty;

        // Si la velocidad es cercana a 0
        if (telemetry.Speed <= 1.0)
        {
            if (threshold == "0")
            {
                violationMessage = "Detención no autorizada detectada.";
                return true;
            }
        }
        return false;
    }
}