using CCS.IngestionApi.DTOs;
using CCS.IngestionApi.Interfaces;
using CCS.Shared.Contracts;
using CCS.Shared.Enums;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace CCS.Tests.Integration;

public class IngestionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IPublishEndpoint> _publishMock;
    private readonly Mock<IMasterDataRepository> _repoMock;

    public IngestionApiTests(WebApplicationFactory<Program> factory)
    {
        _publishMock = new Mock<IPublishEndpoint>();
        _repoMock = new Mock<IMasterDataRepository>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders(); // Evita el error de EventLog
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => _publishMock.Object);
                services.AddScoped(_ => _repoMock.Object);
            });
        });
    }

    // --- TEST 1: TELEMETRÍA (Éxito) ---
    [Fact]
    public async Task Post_Telemetry_Should_Return_Accepted()
    {
        var client = _factory.CreateClient();
        var request = new TelemetryRequest("TEST-CAR", 60, 6.2, -75.5, 90, new());

        var response = await client.PostAsJsonAsync("/api/telemetry", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        _publishMock.Verify(x => x.Publish(It.IsAny<TelemetryReceived>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- TEST 2: TELEMETRÍA (Validación) ---
    [Fact]
    public async Task Post_Telemetry_Invalid_Data_Returns_BadRequest()
    {
        var client = _factory.CreateClient();
        var request = new TelemetryRequest("", 60, 0, 0, 0, null); // Placa vacía

        var response = await client.PostAsJsonAsync("/api/telemetry", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // --- TEST 3: VEHÍCULOS (Éxito) ---
    [Fact]
    public async Task Post_Vehicle_Should_Return_Created()
    {
        var client = _factory.CreateClient();
        var request = new CreateVehicleRequest("NEW-CAR", VehicleType.Car, 1);

        // Simulamos que el dueño existe
        _repoMock.Setup(r => r.OwnerExistsAsync(1)).ReturnsAsync(true);

        var response = await client.PostAsJsonAsync("/api/vehicles", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _repoMock.Verify(r => r.CreateVehicleAsync(It.IsAny<CreateVehicleRequest>()), Times.Once);
    }

    // --- TEST 4: VEHÍCULOS (Error de Dueño Inexistente) --- NUEVO
    [Fact]
    public async Task Post_Vehicle_OwnerNotFound_Returns_BadRequest()
    {
        var client = _factory.CreateClient();
        var request = new CreateVehicleRequest("NEW-CAR", VehicleType.Car, 99);

        // Simulamos que el dueño NO existe
        _repoMock.Setup(r => r.OwnerExistsAsync(99)).ReturnsAsync(false);

        var response = await client.PostAsJsonAsync("/api/vehicles", request);

        // Debe fallar con 400 Bad Request
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("OwnerId no existe");
    }

    // --- TEST 5: OWNERS (Éxito) --- NUEVO
    [Fact]
    public async Task Post_Owner_Should_Return_Created()
    {
        var client = _factory.CreateClient();
        var request = new CreateOwnerRequest("Empresa Test", "test@empresa.com");

        // Simulamos que la BD devuelve el ID 10
        _repoMock.Setup(r => r.CreateOwnerAsync(It.IsAny<CreateOwnerRequest>())).ReturnsAsync(10);

        var response = await client.PostAsJsonAsync("/api/owners", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        // Verificar que devuelve la URL correcta en el header Location o en el body
        var text = await response.Content.ReadAsStringAsync();
        text.Should().Contain("10");
    }

    // --- TEST 6: OWNERS (Error Interno / Excepción) --- NUEVO
    [Fact]
    public async Task Post_Owner_DbError_Returns_500()
    {
        var client = _factory.CreateClient();
        var request = new CreateOwnerRequest("Empresa Fail", "fail@empresa.com");

        // Simulamos que la BD explota
        _repoMock.Setup(r => r.CreateOwnerAsync(It.IsAny<CreateOwnerRequest>()))
                 .ThrowsAsync(new Exception("Error de conexión BD"));

        var response = await client.PostAsJsonAsync("/api/owners", request);

        // Debe manejar la excepción y devolver 500 Problem
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // --- TEST 7: RULES (Éxito) --- NUEVO
    [Fact]
    public async Task Post_Rule_Should_Return_Created()
    {
        var client = _factory.CreateClient();
        var request = new CreateRuleRequest(RuleType.MaxSpeed, "100");
        var plate = "TEST-CAR";

        var response = await client.PostAsJsonAsync($"/api/vehicles/{plate}/rules", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        _repoMock.Verify(r => r.CreateRuleAsync(plate, It.IsAny<CreateRuleRequest>()), Times.Once);
    }
}