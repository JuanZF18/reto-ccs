using CCS.RulesEngine.Strategies;
using CCS.Shared.Contracts;
using FluentAssertions;

namespace CCS.Tests.Strategies;

public class StopEvaluatorTests
{
    private readonly StopEvaluator _evaluator;

    public StopEvaluatorTests()
    {
        _evaluator = new StopEvaluator();
    }

    [Theory]
    [InlineData(0, true)]    // Totalmente quieto -> Alerta
    [InlineData(0.5, true)]  // Casi quieto (<= 1.0) -> Alerta
    [InlineData(20, false)]  // Moviéndose -> OK
    public void Should_Detect_Unplanned_Stops(double speed, bool expected)
    {
        // Arrange
        var telemetry = new TelemetryReceived("TEST-1", 0, 0, speed, 0, DateTime.UtcNow, new());

        // Act
        // El threshold "0" indica que está prohibido detenerse (tolerancia cero)
        var result = _evaluator.Evaluate(telemetry, "0", out string msg);

        // Assert
        result.Should().Be(expected);
        if (expected) msg.Should().Contain("Detención no autorizada");
    }
}