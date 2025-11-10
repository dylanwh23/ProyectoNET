
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobStorageSeeder
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IWebHostEnvironment _env; // Para encontrar la carpeta wwwroot
    private readonly ILogger<BlobStorageSeeder> _logger;

    public BlobStorageSeeder(
        BlobServiceClient blobServiceClient, 
        IWebHostEnvironment env,
        ILogger<BlobStorageSeeder> logger)
    {
        _blobServiceClient = blobServiceClient;
        _env = env;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        const string containerName = "default";
        const string fileName = "carreradefault.png";

        try
        {
            // 1. Obtener el contenedor y crearlo si no existe (con acceso público)
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // 2. Obtener la referencia al blob
            var blobClient = containerClient.GetBlobClient(fileName);

            // 3. Verificar si el blob ya existe
            if (await blobClient.ExistsAsync())
            {
                _logger.LogInformation("El placeholder de imagen ya existe. No se necesita 'seeding'.");
                return;
            }

            // 4. Si no existe, encontrar el archivo en wwwroot
            var localFilePath = Path.Combine(_env.WebRootPath, "seed-images", fileName);

            if (!File.Exists(localFilePath))
            {
                _logger.LogWarning("El archivo placeholder no se encontró en wwwroot/seed-images");
                return;
            }

            // 5. Subirlo
            _logger.LogInformation("Subiendo placeholder de imagen al emulador...");
            await using (var fileStream = File.OpenRead(localFilePath))
            {
                await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/png" });
            }
            
            _logger.LogInformation("¡Placeholder subido con éxito!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el 'seeding' del Storage.");
        }
    }
}