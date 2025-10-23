public record CreateCarreraDTO
{
    public string Nombre { get; init; }
    public DateTime? FechaInicio { get; init; }
    public DateTime? FechaFin { get; init; }
    public long CostoInscripcion { get; init; }

}