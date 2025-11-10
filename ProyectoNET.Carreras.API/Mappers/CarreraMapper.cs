using Riok.Mapperly.Abstractions;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Models;
namespace ProyectoNET.Carreras.API.Mappers;
[Mapper]
public partial class CarreraMapper
{
    public partial Carrera ToEntity(CreateCarreraDTO dto);
    public partial GetCarreraDTO ToDTO(Carrera entity);
    public partial GetCarrerasDTO ToCarrerasDTO(Carrera entity);
    public partial void UpdateEntity(UpdateCarreraDTO dto, Carrera entity);
}