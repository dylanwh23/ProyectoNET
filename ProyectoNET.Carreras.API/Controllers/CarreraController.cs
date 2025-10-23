using Microsoft.AspNetCore.Mvc;
using ProyectoNET.Carreras.API.Models.Repositories;
public class CarreraController : ControllerBase
{
    private readonly ICarreraRepository _carreraRepository;
    private readonly IParticipanteRepository _participanteRepository;
    private readonly ILugarDeEntregaRepository _lugarDeEntregaRepository;

    public CarreraController(
        ICarreraRepository carreraRepository,
        IParticipanteRepository participanteRepository,
        ILugarDeEntregaRepository lugarDeEntregaRepository)
    {
        _carreraRepository = carreraRepository;
        _participanteRepository = participanteRepository;
        _lugarDeEntregaRepository = lugarDeEntregaRepository;
    }

}