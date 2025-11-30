using CCS.RulesEngine.Strategies;
using CCS.Shared.Constants;
using CCS.Shared.Contracts;
using FluentAssertions;

namespace CCS.Tests.Strategies;

public class PanicEvaluatorTests
{
    private readonly PanicEvaluator _evaluator;

    public PanicEvaluatorTests()
    {
        _evaluator = new PanicEvaluator();
    }

    [Fact]
    public void Should_Detect_Panic_Button()
    {
        // Arrange
        var meta = new Dictionary<string, object> { { TelemetryKeys.PanicButton, true } };
        var telemetry = CreateTelemetry(meta);

        // Act
        var result = _evaluator.Evaluate(telemetry, "TRUE", out string msg);

        // Assert
        result.Should().BeTrue();
        msg.Should().Contain("Botón de Pánico Activado");
    }

    [Fact]
    public void Should_Detect_Mechanic_Help()
    {
        // Arrange
        var meta = new Dictionary<string, object> { { TelemetryKeys.MechanicHelp, "true" } };
        var telemetry = CreateTelemetry(meta);

        // Act
        var result = _evaluator.Evaluate(telemetry, "TRUE", out string msg);

        // Assert
        result.Should().BeTrue();
        msg.Should().Contain("Ayuda Mecánica");
    }

    [Fact]
    public void Should_Detect_Accident_By_Vibration()
    {
        // Arrange (Simulamos un choque fuerte de 15G)
        var meta = new Dictionary<string, object> { { TelemetryKeys.CargoVibration, 15.5 } };
        var telemetry = CreateTelemetry(meta);

        // Act
        var result = _evaluator.Evaluate(telemetry, "TRUE", out string msg);

        // Assert
        result.Should().BeTrue();
        msg.Should().Contain("Posible Accidente").And.Contain("15.5");
    }

    [Fact]
    public void Should_Include_Video_Evidence()
    {
        // Arrange
        var meta = new Dictionary<string, object>
        {
            { TelemetryKeys.PanicButton, true },
            { TelemetryKeys.VideoEventUrl, "http://video.com/prueba.mp4" }
        };
        var telemetry = CreateTelemetry(meta);

        // Act
        _evaluator.Evaluate(telemetry, "TRUE", out string msg);

        // Assert
        msg.Should().Contain("EVIDENCIA VIDEO: http://video.com/prueba.mp4");
    }

    // Helper para crear telemetría falsa rápido
    private TelemetryReceived CreateTelemetry(Dictionary<string, object> meta)
        => new("TEST-1", 0, 0, 60, 0, DateTime.UtcNow, meta);
}