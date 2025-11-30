using CCS.RulesEngine.Strategies;
using CCS.Shared.Contracts;
using FluentAssertions;

namespace CCS.Tests.Strategies;

public class MaxSpeedEvaluatorTests
{
    private readonly MaxSpeedEvaluator _evaluator;

    public MaxSpeedEvaluatorTests()
    {
        _evaluator = new MaxSpeedEvaluator();
    }

    [Theory]
    [InlineData(100, "80", true)]   // 100 > 80 -> Alerta
    [InlineData(80, "80", false)]   // 80 == 80 -> OK
    [InlineData(60, "80", false)]   // 60 < 80 -> OK
    [InlineData(100, "INVALID", false)] // Threshold roto -> No explota, retorna false
    public void Should_Evaluate_Speed_Correctly(double currentSpeed, string threshold, bool expected)
    {
        // Arrange
        var telemetry = new TelemetryReceived("TEST-1", 0, 0, currentSpeed, 0, DateTime.UtcNow, new());

        // Act
        var result = _evaluator.Evaluate(telemetry, threshold, out string msg);

        // Assert
        result.Should().Be(expected);
        if (expected) msg.Should().Contain("supera el límite");
    }
}