public record GetCarrerasDTO
{
    public int Id { get; init; }
    public string Nombre { get; init; }
    public string Descripcion { get; init; }
    public string ImagenPromocional { get; init; }
    public string Ubicacion { get; init; }
}