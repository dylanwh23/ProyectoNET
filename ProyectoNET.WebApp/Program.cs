using ProyectoNET.WebApp.Components;


var builder = WebApplication.CreateBuilder(args);

// 1. AÑADIR SERVICIOS AL CONTENEDOR.
// Esto sigue siendo correcto y necesario.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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

// 3. MAPEAR LOS COMPONENTES DE BLAZOR.
// ESTE ES EL CAMBIO CLAVE:
// Ahora, .AddInteractiveServerRenderMode() se aplica directamente al componente <App>.
// Esto establece un contexto interactivo global para toda la aplicación desde el principio.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

