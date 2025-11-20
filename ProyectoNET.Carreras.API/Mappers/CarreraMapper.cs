using Riok.Mapperly.Abstractions;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
using ProyectoNET.Shared.AdminWebApp;

namespace ProyectoNET.Carreras.API.Mappers;

[Mapper]
public partial class CarreraMapper
{
    // ✅ 1. Mapeos Automáticos
    public partial GetCarreraDTO ToDTO(Carrera entity);
    public partial void UpdateEntity(UpdateCarreraDTO dto, Carrera entity);
    public partial CarreraEnCursoCard ToCarreraEnCursoCard(Carrera entity);
    public partial CarreraInscripcionCard ToCarreraInscripcionCard(Carrera entity);

    // Este método usará automáticamente la función "MapStringToLugar" de abajo
    public partial Carrera ToEntity(CreateCarreraDTO dto);


    // ✅ 2. SOLUCIÓN DEL ERROR
    // En lugar de mapear la lista entera, le enseñamos a mapear UNO solo.
    // Mapperly usará esto dentro de un bucle automáticamente.
    private LugarDeEntrega MapStringToLugar(string nombre)
    {
        return new LugarDeEntrega
        {
            Nombre = nombre
        };
    }

    // ✅ 3. Mapeos Manuales 
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
            TotalCorredores = entity.CantidadParticipantes,
            TotalPuntosControl = entity.LugaresRetiroEquipamiento?.Count ?? 0
        };
    }

    public CarreraCard ToCarreraCard(Carrera entity)
    {
        return new CarreraCard
        {
            Id = entity.Id,
            Nombre = entity.Nombre,
            Descripcion = entity.Descripcion,
            ImagenPromocional = entity.ImagenPromocional,
            Ubicacion = entity.Ubicacion,
            FechaInicio = entity.FechaInicio ?? DateTime.MinValue,
            Estado = MapEstadoCarrera(entity.EstadoCarrera),
            TotalInscriptos = entity.CantidadParticipantes,
            TotalPuntosControl = entity.LugaresRetiroEquipamiento?.Count ?? 0
        };
    }

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