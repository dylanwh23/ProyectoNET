public record GetCarreraDTO
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public DateTime? FechaInicio { get; init; }
    public long CostoInscripcion { get; init; }
    public string Ubicacion { get; init; } = string.Empty;
    public int CantidadParticipantes { get; init; }
    public int CantidadMaximaParticipantes { get; init; }
    public string Estado { get; init; } = "Pendiente"; 
    public int TotalCorredores { get; init; } 

}