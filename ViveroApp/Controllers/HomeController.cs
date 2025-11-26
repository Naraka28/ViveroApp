using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using ViveroApp.DTOs;
using ViveroApp.Models;
using ViveroApp.Servicios;

namespace ViveroApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepositorioMiJardin _repositorioMiJardin;

    public HomeController(ILogger<HomeController> logger, IRepositorioMiJardin repositorioMiJardin)
    {
        _logger = logger;
        _repositorioMiJardin = repositorioMiJardin;
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<MiJardinDto> plantasJardin = new List<MiJardinDto>();

        // Si el usuario inició sesión, se muestran las plantitas de su jardín
        if (User.Identity.IsAuthenticated)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(usuarioIdClaim) && int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                plantasJardin = await _repositorioMiJardin.ObtenerPlantasUsuario(usuarioId);
            }
        }

        var plantasPopulares = await _repositorioMiJardin.ObtenerPlantasPopulares(3);

        var viewModel = new HomeViewModel
        {
            MiJardin = plantasJardin,
            PlantasPopulares = plantasPopulares
        };

        return View(viewModel);
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

