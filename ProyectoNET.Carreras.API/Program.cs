using MassTransit;
using ProyectoNET.Carreras.API.Consumers;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Shared;
using ProyectoNET.Carreras.API.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. CONFIGURAR SERVICIOS (Contenedor de Inyección de Dependencias)
// =================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.AddNpgsqlDbContext<CarrerasDbContext>("carreras-db");


// Configuración de MassTransit (se mantiene igual)
builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<TiempoRegistradoConsumer>();
    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
        cfg.ReceiveEndpoint("tiempos-queue", e =>
        {
            e.ConfigureConsumer<TiempoRegistradoConsumer>(ctx);
        });
    });
});

// *** CAMBIO CLAVE 1: CORRECCIÓN EN LA POLÍTICA DE CORS ***
// Se usa una política con nombre para ser más explícitos y evitar conflictos.
// Asegúrate de que el puerto "7072" coincida con el de tu WebApp. En tu screenshot era 7072.
var corsPolicyName = "WebAppPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins("https://localhost:7073") // Puerto de tu WebApp
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Crucial para SignalR
    });
});

// ===============================================
// 2. CONSTRUIR LA APLICACIÓN
// ===============================================
var app = builder.Build();

// =================================================================
// 3. CONFIGURAR EL PIPELINE DE PETICIONES HTTP (Middleware)
// ¡EL ORDEN AQUÍ ES MUY IMPORTANTE!
// =================================================================

// Configuración para el entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// *** CAMBIO CLAVE 2: POSICIÓN CORRECTA DEL MIDDLEWARE DE CORS ***
// Debe ir ANTES de la autenticación/autorización y de mapear los endpoints.
app.UseCors(corsPolicyName);

app.UseAuthorization();

// Mapeo de los endpoints (Hub de SignalR y Controladores)
app.MapHub<CarreraHub>("/carreraHub");
app.MapControllers();

// Endpoints de Minimal API (se mantienen igual)
app.MapPost("/carrera/iniciar", async (IniciarCarreraCommand command, IBus bus) =>
{
    var endpoint = await bus.GetSendEndpoint(new Uri("queue:simulador-carreras"));
    await endpoint.Send(command);
    return Results.Accepted(value: new { message = $"Comando para iniciar la carrera {command.IdCarrera} enviado." });
})
.WithSummary("Inicia la simulación de una carrera.")
.WithName("IniciarCarrera")
.WithOpenApi();

app.MapGet("/carreras-test", () => "Respuesta del Microservicio de Carreras");

// para aplicar las migraciones al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<CarrerasDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al aplicar las migraciones.");
    }
}
/**/

// ===============================================
// 4. EJECUTAR LA APLICACIÓN
// ===============================================
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
