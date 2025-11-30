using CCS.Shared.Contracts;
using CCS.Shared.Enums;

namespace CCS.RulesEngine.Interfaces;

public interface IRuleEvaluator
{
    RuleType Type { get; }
    bool Evaluate(TelemetryReceived telemetry, string threshold, out string violationMessage);
}