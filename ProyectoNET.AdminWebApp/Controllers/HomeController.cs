using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProyectoNET.AdminWebApp.Models;

namespace ProyectoNET.AdminWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        // Leer la URL base de la API desde appsettings.json
        var apiUrl = _configuration["ApiSettings:BaseUrl"];
        ViewData["ApiUrl"] = apiUrl;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
