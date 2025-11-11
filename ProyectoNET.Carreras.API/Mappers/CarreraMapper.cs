using Riok.Mapperly.Abstractions;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
namespace ProyectoNET.Carreras.API.Mappers;
[Mapper]
public partial class CarreraMapper
{
    public partial Carrera ToEntity(CreateCarreraDTO dto);
    public partial GetCarreraDTO ToDTO(Carrera entity);
    public partial ProyectoNET.Shared.CarreraCard ToCarrerasDTO(Carrera entity);
    public partial void UpdateEntity(UpdateCarreraDTO dto, Carrera entity);

    public partial CarreraEnCursoCard ToCarreraEnCursoCard(Carrera entity);
    public partial CarreraInscripcionCard ToCarreraInscripcionCard(Carrera entity);
}