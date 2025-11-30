using CCS.RulesEngine.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;
using MassTransit;

namespace CCS.RulesEngine.Consumers;

public class TelemetryConsumer : IConsumer<TelemetryReceived>
{
    private readonly IRulesRepository _repository;
    private readonly IEnumerable<IRuleEvaluator> _evaluators;

    public TelemetryConsumer(IRulesRepository repository, IEnumerable<IRuleEvaluator> evaluators)
    {
        _repository = repository;
        _evaluators = evaluators;
    }

    public async Task Consume(ConsumeContext<TelemetryReceived> context)
    {
        var msg = context.Message;
        var alerts = new List<AlertTriggered>();

        var rules = _repository.GetRulesForVehicle(msg.VehiclePlate);

        foreach (var rule in rules)
        {
            var evaluator = _evaluators.FirstOrDefault(e => e.Type == (RuleType)rule.rule_type);

            if (evaluator != null)
            {
                if (evaluator.Evaluate(msg, rule.threshold_value, out string errorMsg))
                {
                    alerts.Add(new AlertTriggered(msg.VehiclePlate, errorMsg, DateTime.UtcNow));
                }
            }
        }

        foreach (var alert in alerts)
        {
            await context.Publish(alert);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ALERTA GENERADA] {alert.VehiclePlate}: {alert.RuleDescription}");
            Console.ResetColor();

        }
    }
}