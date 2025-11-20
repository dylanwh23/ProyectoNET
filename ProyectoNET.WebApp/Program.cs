using ProyectoNET.WebApp.Components;
using ProyectoNET.WebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpClient("api", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiUrl = config["services:carreras-api:https:0"] ?? config["services:carreras-api:http:0"];

    if (string.IsNullOrEmpty(apiUrl))
    {
        throw new InvalidOperationException("No se pudo encontrar la URL del servicio 'carreras-api'.");
    }
    
    client.BaseAddress = new Uri(apiUrl);
});

// Configure HttpClient for Usuarios.API
builder.Services.AddHttpClient("usuariosApi", (sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiUrl = config["services:usuarios-api:https:0"] ?? config["services:usuarios-api:http:0"];

    if (string.IsNullOrEmpty(apiUrl))
    {
        throw new InvalidOperationException("No se pudo encontrar la URL del servicio 'usuarios-api'.");
    }
    
    client.BaseAddress = new Uri(apiUrl);
});

builder.AddRedisClient("redis");

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 5 * 1024 * 1024; 
    hubOptions.EnableDetailedErrors = true;
})
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("redis")!);

var app = builder.Build();

// 2. CONFIGURAR EL PIPELINE DE PETICIONES HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<CarrerasHub>("/carrerasHub");

// 3. MAPEAR LOS COMPONENTES DE BLAZOR.
// ESTE ES EL CAMBIO CLAVE:
// Ahora, .AddInteractiveServerRenderMode() se aplica directamente al componente <App>.
// Esto establece un contexto interactivo global para toda la aplicación desde el principio.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
