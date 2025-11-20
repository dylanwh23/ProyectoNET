using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProyectoNET.Carreras.API.Consumers;
using ProyectoNET.Carreras.API.Data;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Carreras.API.Mappers;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Carreras.API.Consumers;
using ProyectoNET.Shared.EventosRabbit;
using ProyectoNET.Carreras.API.Services;
 


var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. CONFIGURAR SERVICIOS (Contenedor de Inyección de Dependencias)
// =================================================================
builder.Services.AddScoped<IGeoProcessingService, GeoProcessingService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddControllers();

// Bases de datos y Caché
builder.AddNpgsqlDbContext<CarrerasDbContext>("carreras-db");
builder.AddRedisClient("redis");

// =================================================================
// CONFIGURACIÓN DE MASSTRANSIT (CORREGIDA)
// =================================================================
builder.Services.AddMassTransit(config =>
{
    // Registrar el Consumer
    config.AddConsumer<UsuarioInscritoEventConsumer>();
    config.AddConsumer<CarreraFinalizadaConsumer>();
    config.UsingRabbitMq((context, cfg) =>
    {
        // Obtenemos la cadena de conexión que inyecta .NET Aspire / Docker
        // Ejemplo: "amqp://guest:guest@localhost:60035"
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq-bus");

        if (!string.IsNullOrEmpty(rabbitMqConnectionString))
        {
            // ✅ SOLUCIÓN: Usamos directamente la URI. 
            // MassTransit detectará automáticamente el puerto dinámico (ej. 60035)
            cfg.Host(new Uri(rabbitMqConnectionString));
        }
        else
        {
            // Fallback por defecto (solo si no hay cadena de conexión)
            cfg.Host("localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        }
        cfg.ReceiveEndpoint("carrera-finalizacion", e =>
        {
            e.Durable = true;
            e.AutoDelete = false;
            e.ConfigureConsumer<CarreraFinalizadaConsumer>(context);
        });
        // Configuración del endpoint para la cola específica
        cfg.ReceiveEndpoint("usuario-inscrito-carrera-queue", e =>
        {
            e.ConfigureConsumer<UsuarioInscritoEventConsumer>(context);
        });
        
        // Configuración general de endpoints
        cfg.ConfigureEndpoints(context); 
    });
});

// =================================================================
// CONFIGURACIÓN DE CORS
// =================================================================
var corsPolicyName = "WebAppPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins(
            "https://localhost:7073",   // WebApp original
            "https://localhost:5001",   // Blazor o MVC
            "http://127.0.0.1:5500",    // Live Server de VSCode
            "https://localhost:7188",
            "https://localhost:7182",
            "https://localhost:7072"    // Tu WebApp actual
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Crucial para SignalR
    });
});

// =================================================================
// REPOSITORIOS Y SERVICIOS
// =================================================================
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IParticipanteRepository, ParticipanteRepository>();
builder.Services.AddScoped<ILugarDeEntregaRepository, LugarDeEntregaRepository>();

// Mapperly
builder.Services.AddSingleton<CarreraMapper>();

// Cliente Blob Storage (Imágenes)
builder.AddAzureBlobServiceClient("blobstorage");
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddTransient<BlobStorageSeeder>(); 

// ===============================================
// 2. CONSTRUIR LA APLICACIÓN
// ===============================================
var app = builder.Build();

// =================================================================
// 3. PIPELINE HTTP
// =================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CORS debe ir ANTES de Auth y MapControllers
app.UseCors(corsPolicyName);

app.UseAuthorization();

// Endpoints
app.MapHub<CarreraHub>("/carreraHub");
app.MapControllers();

// =================================================================
// 4. MIGRACIONES Y SEEDERS
// =================================================================


// para aplicar las migraciones al iniciar la aplicación
const int maxRetries = 5;
TimeSpan delay = TimeSpan.FromSeconds(5);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    // Bucle de reintento para garantizar que la DB de Aspire esté levantada
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var dbContext = services.GetRequiredService<CarrerasDbContext>();
            await dbContext.Database.MigrateAsync();
            break; 
        }
        catch (Exception ex)
        {
            if (i < maxRetries - 1)
            {
                logger.LogWarning(ex, "❌ Falló el intento de acceso a la DB. Reintentando en {DelaySeconds} segundos...", delay.TotalSeconds);
                await Task.Delay(delay); 
            }
            else
            {
                logger.LogError(ex, "❌ ERROR FATAL: No se pudo conectar a la DB de Aspire después de {MaxRetries} intentos.", maxRetries);
                // Aquí, la app fallará al iniciar si no puede conectar
                throw; 
            }
        }
    }
}

// Inicializar Blob Storage (Seed)
if (app.Environment.IsDevelopment())
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<BlobStorageSeeder>();
            await seeder.InitializeAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al inicializar el Blob Storage Seeder.");
    }
}

// ===============================================
// 5. EJECUTAR
// ===============================================
app.Run();