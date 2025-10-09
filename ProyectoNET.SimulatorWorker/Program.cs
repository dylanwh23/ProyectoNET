using MassTransit;
using ProyectoNET.SimulatorWorker;
using ProyectoNET.SimulatorWorker.Consumers;
var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults(); 
builder.Services.AddMassTransit(config =>
{
    // Registra nuestro consumidor para que MassTransit lo conozca
    config.AddConsumer<IniciarCarreraConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        // Lee la conexión del bus inyectada por Aspire
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));

        // Configura la cola para recibir los comandos y la conecta con nuestro consumidor
        cfg.ReceiveEndpoint("simulador-carreras", e =>
        {
            e.ConfigureConsumer<IniciarCarreraConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();