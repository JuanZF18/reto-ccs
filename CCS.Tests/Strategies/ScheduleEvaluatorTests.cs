using CCS.RulesEngine.Strategies;
using CCS.Shared.Contracts;
using FluentAssertions;

namespace CCS.Tests.Strategies;

public class ScheduleEvaluatorTests
{
    private readonly ScheduleEvaluator _evaluator;

    public ScheduleEvaluatorTests()
    {
        _evaluator = new ScheduleEvaluator();
    }

    [Theory]
    // 19:30 UTC es 14:30 Colombia. El rango prohibido es 14:00-16:00. DEBE ALERTAR (True).
    [InlineData("14:00-16:00", 19, 30, true)]

    // 15:00 UTC es 10:00 Colombia. El rango prohibido es 14:00-16:00. NO DEBE ALERTAR (False).
    [InlineData("14:00-16:00", 15, 00, false)]

    // Rango Nocturno (23:00-05:00). 06:00 UTC es 01:00 Colombia. DEBE ALERTAR (True).
    [InlineData("23:00-05:00", 06, 00, true)]
    public void Should_Evaluate_Schedule_Correctly(string ruleRange, int utcHour, int utcMinute, bool expectedAlert)
    {
        // Arrange
        var time = DateTime.UtcNow.Date.AddHours(utcHour).AddMinutes(utcMinute);
        // Velocidad 60 para que cuente como movimiento
        var telemetry = new TelemetryReceived("TAX-1", 0, 0, 60, 0, time, new());

        // Act
        var result = _evaluator.Evaluate(telemetry, ruleRange, out string msg);

        // Assert
        result.Should().Be(expectedAlert);
    }

    [Fact]
    public void Should_Ignore_Stopped_Vehicle()
    {
        // Arrange: Vehículo detenido (Speed 0) en horario prohibido
        var time = DateTime.UtcNow;
        var telemetry = new TelemetryReceived("TAX-1", 0, 0, 0, 0, time, new());

        // Act
        var result = _evaluator.Evaluate(telemetry, "00:00-23:59", out _);

        // Assert
        result.Should().BeFalse("Un vehículo detenido no debe generar alerta de horario");
    }
}