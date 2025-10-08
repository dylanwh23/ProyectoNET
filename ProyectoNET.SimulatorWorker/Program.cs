using MassTransit;
using ProyectoNET.SimulatorWorker;

var builder = Host.CreateApplicationBuilder(args);

// Configurar MassTransit
builder.Services.AddMassTransit(config => {
    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
    });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();