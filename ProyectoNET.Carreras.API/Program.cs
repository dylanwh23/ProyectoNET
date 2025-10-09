using MassTransit;
using ProyectoNET.Carreras.API.Consumers;
using ProyectoNET.Shared;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(config => {
    config.AddConsumer<TiempoRegistradoConsumer>(); // <-- Registra tu consumidor
    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));

        // Configura el endpoint para el consumidor
        cfg.ReceiveEndpoint("tiempos-queue", e => {
            e.ConfigureConsumer<TiempoRegistradoConsumer>(ctx);
        });
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapPost("/carrera/iniciar", async (IniciarCarreraCommand command, IBus bus) =>
{
    // Obtiene el endpoint de la cola específica para los simuladores
    var endpoint = await bus.GetSendEndpoint(new Uri("queue:simulador-carreras"));

    // Envía el comando a esa cola
    await endpoint.Send(command);

    return Results.Accepted(value: new { message = $"Comando para iniciar la carrera {command.IdCarrera} enviado." });
})
.WithSummary("Inicia la simulación de una carrera.")
.WithName("IniciarCarrera")
.WithOpenApi();

app.MapGet("/carreras-test", () => "Respuesta del Microservicio de Carreras");


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
