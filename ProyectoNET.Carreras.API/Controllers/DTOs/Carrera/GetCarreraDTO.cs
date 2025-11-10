public record GetCarreraDTO
{
    public int Id { get; init; }
    public string Nombre { get; init; }
    public string Descripcion { get; init; }
    public DateTime? FechaInicio { get; init; }
    public long CostoInscripcion { get; init; }
    public string Ubicacion { get; init; }
    public int CantidadParticipantes { get; init; }
    public int CantidadMaximaParticipantes { get; init; }

}