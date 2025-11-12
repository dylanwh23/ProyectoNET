public record GetCarrerasDTO
{
    public int Id { get; init; }
    public required string Nombre { get; init; }
    public required string Descripcion { get; init; }
    public required string ImagenPromocional { get; init; }
    public required string Ubicacion { get; init; }
    public string Estado { get; init; } = "Pendiente";
    public int TotalCorredores { get; init; }
}