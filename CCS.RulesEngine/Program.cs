using CCS.RulesEngine.Consumers;
using CCS.RulesEngine.Data;
using CCS.RulesEngine.Interfaces;
using CCS.RulesEngine.Strategies;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

// 1. Registrar Servicios
builder.Services.AddSingleton<IRulesRepository, RulesRepository>();
builder.Services.AddSingleton<IRuleEvaluator, MaxSpeedEvaluator>();
builder.Services.AddSingleton<IRuleEvaluator, PanicEvaluator>();
builder.Services.AddSingleton<IRuleEvaluator, TemperatureEvaluator>();
builder.Services.AddSingleton<IRuleEvaluator, StopEvaluator>();
builder.Services.AddSingleton<IRuleEvaluator, ScheduleEvaluator>();
builder.Services.AddSingleton<IRuleEvaluator, DoorSensorEvaluator>();

// 2. Configurar MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TelemetryConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h => { h.Username("guest"); h.Password("guest"); });

        cfg.ReceiveEndpoint("telemetry-queue", e =>
        {
            e.PrefetchCount = 20;
            e.ConfigureConsumer<TelemetryConsumer>(context);
        });
    });
});

var host = builder.Build();

var repo = host.Services.GetRequiredService<IRulesRepository>();
await repo.LoadRulesAsync();

host.Run();