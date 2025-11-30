using CCS.Shared.Contracts;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AlertConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h => { h.Username("guest"); h.Password("guest"); });

        cfg.ReceiveEndpoint("notifications-queue", e =>
        {
            e.ConfigureConsumer<AlertConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();

// El Consumidor
public class AlertConsumer : IConsumer<AlertTriggered>
{
    public Task Consume(ConsumeContext<AlertTriggered> context)
    {
        //var alert = context.Message;
        //Console.BackgroundColor = ConsoleColor.DarkGreen;
        //Console.WriteLine($"[EMAIL ENVIADO] Propietario de {alert.VehiclePlate} notificado: {alert.RuleDescription}");
        //Console.ResetColor();
        return Task.CompletedTask;
    }
}