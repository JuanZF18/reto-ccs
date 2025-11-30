using CCS.RulesEngine.Consumers;
using CCS.RulesEngine.Interfaces;
using CCS.RulesEngine.Models;
using CCS.RulesEngine.Strategies;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;
using FluentAssertions;
using MassTransit;
using Moq;

namespace CCS.Tests.Consumers;

public class TelemetryConsumerTests
{
    private readonly Mock<IRulesRepository> _repoMock;
    private readonly Mock<ConsumeContext<TelemetryReceived>> _contextMock;
    private readonly List<IRuleEvaluator> _strategies;
    private readonly TelemetryConsumer _consumer;

    public TelemetryConsumerTests()
    {
        // 1. Mock del Repositorio
        _repoMock = new Mock<IRulesRepository>();

        // 2. Usamos la estrategia real de Velocidad (ya sabemos que funciona)
        _strategies = new List<IRuleEvaluator> { new MaxSpeedEvaluator() };

        // 3. Mock del Contexto de MassTransit (El mensaje que llega)
        _contextMock = new Mock<ConsumeContext<TelemetryReceived>>();

        // 4. Instanciar el consumidor real con dependencias falsas
        _consumer = new TelemetryConsumer(_repoMock.Object, _strategies);
    }

    [Fact]
    public async Task Should_Trigger_Alert_When_Rule_Is_Broken()
    {
        // ARRANGE
        // Datos de prueba: Un vehículo que va a 100km/h
        var message = new TelemetryReceived("TEST-CAR", 0, 0, 100, 0, DateTime.UtcNow, new());

        // Configuramos el Mock de MassTransit para que devuelva ese mensaje
        _contextMock.Setup(x => x.Message).Returns(message);

        // Configuramos el Repo para que diga "Este carro tiene límite de 80"
        _repoMock.Setup(r => r.GetRulesForVehicle("TEST-CAR"))
            .Returns(new List<RuleConfigDbModel>
            {
                new RuleConfigDbModel { rule_type = (int)RuleType.MaxSpeed, threshold_value = "80" }
            });

        // ACT
        await _consumer.Consume(_contextMock.Object);

        // ASSERT
        // Verificamos que el consumidor haya intentado Publicar (Publish) una alerta
        _contextMock.Verify(x => x.Publish(
            It.Is<AlertTriggered>(a => a.VehiclePlate == "TEST-CAR" && a.RuleDescription.Contains("supera el límite")),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task Should_NOT_Trigger_Alert_When_Rules_Are_Obeyed()
    {
        // ARRANGE
        // Vehículo a 60km/h (bajo el límite)
        var message = new TelemetryReceived("TEST-CAR", 0, 0, 60, 0, DateTime.UtcNow, new());
        _contextMock.Setup(x => x.Message).Returns(message);

        _repoMock.Setup(r => r.GetRulesForVehicle("TEST-CAR"))
            .Returns(new List<RuleConfigDbModel>
            {
                new RuleConfigDbModel { rule_type = (int)RuleType.MaxSpeed, threshold_value = "80" }
            });

        // ACT
        await _consumer.Consume(_contextMock.Object);

        // ASSERT
        // Verificamos que NUNCA se llamó a Publish
        _contextMock.Verify(x => x.Publish(It.IsAny<AlertTriggered>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}