using ProyectoNET.AdminWebApp.Components;
using ProyectoNET.AdminWebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["ApiUrls:CarrerasApi"] ?? "https://localhost:7252")
});


builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting(); //  DEBE estar aquí
    
app.UseAuthentication();
app.UseAuthorization(); // DESPUÉS de UseRouting()
app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorComponents<ProyectoNET.AdminWebApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
