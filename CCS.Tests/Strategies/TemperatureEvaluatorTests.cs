using CCS.RulesEngine.Strategies;
using CCS.Shared.Constants;
using CCS.Shared.Contracts;
using FluentAssertions;

namespace CCS.Tests.Strategies;

public class TemperatureEvaluatorTests
{
    private readonly TemperatureEvaluator _evaluator;

    public TemperatureEvaluatorTests()
    {
        _evaluator = new TemperatureEvaluator();
    }

    [Fact]
    public void Should_Alert_If_Temperature_Exceeds_Threshold()
    {
        // Arrange: Carga a 10 grados, Límite es -5
        var meta = new Dictionary<string, object> { { TelemetryKeys.CargoTemperature, 10.0 } };
        var telemetry = new TelemetryReceived("TEST-1", 0, 0, 60, 0, DateTime.UtcNow, meta);

        // Act
        var result = _evaluator.Evaluate(telemetry, "-5", out string msg);

        // Assert
        result.Should().BeTrue();
        msg.Should().Contain("Ruptura de Cadena de Frío");
    }

    [Fact]
    public void Should_Pass_If_Temperature_Is_Low()
    {
        // Arrange: Carga a -10 grados, Límite es -5 -> OK
        var meta = new Dictionary<string, object> { { TelemetryKeys.CargoTemperature, -10.0 } };
        var telemetry = new TelemetryReceived("TEST-1", 0, 0, 60, 0, DateTime.UtcNow, meta);

        // Act
        var result = _evaluator.Evaluate(telemetry, "-5", out string msg);

        // Assert
        result.Should().BeFalse();
        msg.Should().BeEmpty();
    }

    [Fact]
    public void Should_Return_False_If_No_Sensor_Data()
    {
        // Arrange: No enviamos metadata de temperatura
        var telemetry = new TelemetryReceived("TEST-1", 0, 0, 60, 0, DateTime.UtcNow, new());

        // Act
        var result = _evaluator.Evaluate(telemetry, "-5", out _);

        // Assert
        result.Should().BeFalse("Sin datos de sensor no se puede evaluar");
    }
}