using Microsoft.EntityFrameworkCore;
using ProyectoNET.Usuarios.API.Data;
using ProyectoNET.Usuarios.API.Services; // Añadir este using
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// =================================================================
// 1. CONFIGURAR SERVICIOS
// =================================================================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Configurar PostgreSQL
builder.AddNpgsqlDbContext<UsuariosDbContext>("usuarios-db");

// Configurar servicio de email
builder.Services.AddTransient<IEmailService, EmailService>(); // Registrar EmailService


// =================================================================
// CONFIGURACIÓN DE MASSTRANSIT (CORREGIDA)
// =================================================================
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        // Obtenemos la cadena de conexión de .NET Aspire / Docker
        var rabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq-bus");

        // ✅ SOLUCIÓN: Usamos directamente la URI.
        if (!string.IsNullOrEmpty(rabbitMqConnectionString))
        {
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
        
        cfg.ConfigureEndpoints(context);
    });
});

// =================================================================
// CONFIGURACIÓN DE CORS
// =================================================================
var corsPolicyName = "AllowWebApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins(
            "https://localhost:7073",
            "https://localhost:7182",
            "http://localhost:5001",
            "https://localhost:7072" // Agregado por consistencia con tus otros servicios
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ===============================================
// 2. CONSTRUIR Y CONFIGURAR PIPELINE
// ===============================================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthorization();
app.MapControllers();

// ===============================================
// 3. MIGRACIONES
// ===============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<UsuariosDbContext>();
        dbContext.Database.Migrate();
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("✅ Migraciones aplicadas correctamente en Usuarios.API");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Ocurrió un error al aplicar las migraciones.");
    }
}

app.Run();
