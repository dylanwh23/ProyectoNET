using MassTransit;
using ProyectoNET.SimulatorWorker.Consumers;
using ProyectoNET.Shared.WebApp; // <-- ¡Asegúrate de tener el using a tus eventos!
using RabbitMQ.Client; // <-- ¡Asegúrate de tener el using a RabbitMQ!
using ProyectoNET.Shared;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<IniciarCarreraConsumer>();
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
        cfg.ConfigureEndpoints(context);
    });
});
var host = builder.Build();
host.Run();