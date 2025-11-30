using CCS.IngestionApi.Data;
using CCS.IngestionApi.DTOs;
using CCS.IngestionApi.Interfaces;
using CCS.Shared.Contracts;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configuración de Servicios ---

// Habilita el explorador de endpoints (necesario para Minimal APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IMasterDataRepository, MasterDataRepository>();
// Configuración de Swagger Gen (OpenAPI 3.0)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CCS Ingestion API",
        Version = "v1",
        Description = "API de alto rendimiento para la recepción de telemetría vehicular (GPS, Sensores, Alarmas).",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Arquitectura CCS",
            Email = "arquitectura@ccs-tracking.com"
        }
    });

    // Incluir comentarios XML (Busca el archivo generado por el .csproj)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Configuración MassTransit (Igual que antes)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });
    });
});

var app = builder.Build();

// --- 2. Pipeline de Middleware ---

// Habilitar Swagger UI en entorno de desarrollo (o siempre, según política)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CCS Ingestion API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapPost("/api/telemetry", async ([FromBody] TelemetryRequest request, IPublishEndpoint publishEndpoint) =>
{
    if (string.IsNullOrEmpty(request.Plate))
        return Results.BadRequest(new { error = "La placa es obligatoria." });

    var eventMessage = new TelemetryReceived(
        request.Plate,
        request.Lat,
        request.Lon,
        request.Speed,
        request.Heading,
        DateTime.UtcNow,
        request.Metadata ?? new Dictionary<string, object>()
    );

    await publishEndpoint.Publish(eventMessage);

    return Results.Accepted(value: new { status = "Signal Queued", traceId = Guid.NewGuid() });
})
.WithName("ReceiveTelemetry")
.WithTags("Telemetry")
.WithSummary("Recibe señales de telemetría de vehículos")
.WithDescription("Endpoint de alta velocidad. Valida y encola en RabbitMQ.")
.Produces(StatusCodes.Status202Accepted)
.Produces(StatusCodes.Status400BadRequest);

app.MapPost("/api/owners", async ([FromBody] CreateOwnerRequest req, IMasterDataRepository repo) =>
{
    try
    {
        var id = await repo.CreateOwnerAsync(req);
        return Results.Created($"/api/owners/{id}", new { id });
    }
    catch (Exception ex) { return Results.Problem(ex.Message); }
})
.WithName("CreateOwner")
.WithTags("Management")
.WithSummary("Registra un nuevo propietario")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status500InternalServerError);

app.MapPost("/api/vehicles", async ([FromBody] CreateVehicleRequest req, IMasterDataRepository repo) =>
{
    try
    {
        if (!await repo.OwnerExistsAsync(req.OwnerId))
            return Results.BadRequest("OwnerId no existe.");

        await repo.CreateVehicleAsync(req);
        return Results.Created($"/api/vehicles/{req.Plate}", req);
    }
    catch (PostgresException ex) when (ex.SqlState == "23505")
    {
        return Results.Conflict($"El vehículo {req.Plate} ya existe.");
    }
    catch (Exception ex) { return Results.Problem(ex.Message); }
})
.WithName("CreateVehicle")
.WithTags("Management")
.WithSummary("Registra un nuevo vehículo")
.Produces(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status409Conflict);

app.MapPost("/api/vehicles/{plate}/rules", async (string plate, [FromBody] CreateRuleRequest req, IMasterDataRepository repo) =>
{
    try
    {
        await repo.CreateRuleAsync(plate, req);
        return Results.Created($"/api/vehicles/{plate}/rules", req);
    }
    catch (Exception ex) { return Results.Problem(ex.Message); }
})
.WithName("CreateVehicleRule")
.WithTags("Management")
.WithSummary("Asigna una regla a un vehículo")
.WithDescription("Define límites como velocidad máxima o zonas prohibidas.")
.Produces(StatusCodes.Status201Created);

app.Run();

public partial class Program { }