using CCS.RulesEngine.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;
using System.Text.Json;

namespace CCS.RulesEngine.Strategies;

public class DoorSensorEvaluator : IRuleEvaluator
{
    public RuleType Type => RuleType.DoorSensor;

    public bool Evaluate(TelemetryReceived telemetry, string thresholdValue, out string violationMessage)
    {
        violationMessage = string.Empty;

        if (telemetry.Metadata == null || !telemetry.Metadata.ContainsKey("doorOpen"))
            return false;

        var doorOpenValue = telemetry.Metadata["doorOpen"];
        bool isDoorOpen = false;

        if (doorOpenValue is JsonElement element)
        {
            isDoorOpen = element.ValueKind == JsonValueKind.True;
        }
        else if (doorOpenValue is bool b)
        {
            isDoorOpen = b;
        }

        if (isDoorOpen && telemetry.Speed > 0)
        {
            violationMessage = "Puerta trasera abierta detectada con vehículo en movimiento.";
            return true;
        }

        return false;
    }
}