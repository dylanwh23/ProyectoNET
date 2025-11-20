using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.SimulatorConsumer.Consumers;
using ProyectoNET.Shared.WebApp; // Donde esté tu RaceHub

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddRedisClient("redis");

var redisConn = builder.Configuration.GetConnectionString("redis");
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConn!);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CorredorDataConsumer>();
    x.AddConsumer<CarreraIniciadaConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
        cfg.ReceiveEndpoint("race-events", e =>
        {
            e.Durable = true; 
            e.AutoDelete = false;
            e.ConfigureConsumer<CorredorDataConsumer>(context);
            e.ConfigureConsumer<CarreraIniciadaConsumer>(context);
        });
    });
});

var host = builder.Build();
host.Run();