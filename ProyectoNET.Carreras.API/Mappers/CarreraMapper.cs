using Riok.Mapperly.Abstractions;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;

namespace ProyectoNET.Carreras.API.Mappers;

[Mapper]
public partial class CarreraMapper
{
    //  Mapeos automáticos de Mapperly
    public partial Carrera ToEntity(CreateCarreraDTO dto);
    public partial GetCarreraDTO ToDTO(Carrera entity);

    public partial void UpdateEntity(UpdateCarreraDTO dto, Carrera entity);

    public partial CarreraEnCursoCard ToCarreraEnCursoCard(Carrera entity);
    public partial CarreraInscripcionCard ToCarreraInscripcionCard(Carrera entity);

    //  Mapeo MANUAL para GetCarrerasDTO con Estado
    public GetCarrerasDTO ToCarrerasListDTO(Carrera entity)
    {
        return new GetCarrerasDTO
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            ImagenPromocional = entity.ImagenPromocional,
            Ubicacion = entity.Ubicacion,
            Estado = MapEstadoCarrera(entity.EstadoCarrera),
            TotalCorredores = entity.CantidadParticipantes
        };
    }

    //  Mapeo MANUAL para CarreraCard (con propiedades correctas)
    public CarreraCard ToCarreraCard(Carrera entity)
    {
        return new CarreraCard
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            ImagenPromocional = entity.ImagenPromocional,
            Ubicacion = entity.Ubicacion,
            FechaInicio = entity.FechaInicio ?? DateTime.MinValue, //  Conversión de DateTime? a DateTime
            Estado = MapEstadoCarrera(entity.EstadoCarrera),
            TotalInscriptos = entity.CantidadParticipantes,
            TotalPuntosControl = entity.LugaresRetiroEquipamiento?.Count ?? 0
        };
    }

    //  Método auxiliar para convertir enum a string
    private string MapEstadoCarrera(Carrera.Estado estado)
    {
        return estado switch
        {
            Carrera.Estado.Pendiente => "Pendiente",
            Carrera.Estado.EnProgreso => "En Curso",
            Carrera.Estado.Finalizada => "Finalizada",
            _ => "Pendiente"
        };
    }
}