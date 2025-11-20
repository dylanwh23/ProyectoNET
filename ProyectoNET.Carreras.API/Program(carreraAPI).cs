using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProyectoNET.Carreras.API.Consumers;
using ProyectoNET.Carreras.API.Data;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Carreras.API.Mappers;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Carreras.API.Services;
 

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. CONFIGURAR SERVICIOS (Contenedor de Inyección de Dependencias)
// =================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.AddNpgsqlDbContext<CarrerasDbContext>("carreras-db");
builder.AddRedisClient("redis");
builder.Services.AddScoped<IGeoProcessingService, GeoProcessingService>();

// Configuración de MassTransit (se mantiene igual)
// ...


// Agregar MassTransit
builder.Services.AddMassTransit(config =>
{
    // ⭐ AGREGAR EL CONSUMER
    config.AddConsumer<CarreraFinalizadaConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));

        // ⭐ CONFIGURAR ENDPOINT PARA EL CONSUMER
        cfg.ReceiveEndpoint("carrera-finalizacion", e =>
        {
            e.Durable = true;
            e.AutoDelete = false;
            e.ConfigureConsumer<CarreraFinalizadaConsumer>(context);
        });
    });
});

// ...

// *** CAMBIO CLAVE 1: CORRECCIÓN EN LA POLÍTICA DE CORS ***
// Se usa una política con nombre para ser más explícitos y evitar conflictos.
// Asegúrate de que el puerto "7072" coincida con el de tu WebApp. En tu screenshot era 7072.
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
            "https://localhost:7182"// 
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Crucial para SignalR
    });
});

//Repositorios
builder.Services.AddScoped<ICarreraRepository, CarreraRepository>();
builder.Services.AddScoped<IParticipanteRepository, ParticipanteRepository>();
builder.Services.AddScoped<ILugarDeEntregaRepository, LugarDeEntregaRepository>();

//Mapperly
builder.Services.AddSingleton<CarreraMapper>();

//Cliente blob storage (imagenes)
builder.AddAzureBlobServiceClient("blobstorage");
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddTransient<BlobStorageSeeder>(); // para que se suba la imagen default al blobstorage siempre


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
/**/

// Seeder para el blob storage (imagen default)
if (app.Environment.IsDevelopment())
{
    try
    {
        // Obtenemos el servicio y lo ejecutamos
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
/**/

// ===============================================
// 4. EJECUTAR LA APLICACIÓN
// ===============================================
app.Run();



