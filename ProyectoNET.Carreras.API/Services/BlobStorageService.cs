using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    // Inyectamos el 'BlobServiceClient' que Aspire configuró automáticamente
    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string containerName)
    {
        // 1. Obtiene el "contenedor" (como una carpeta raíz)
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        // 2. Asegura que exista y que sea de acceso público para lectura
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        // 3. Genera un nombre único para el archivo
        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";

        // 4. Obtiene la referencia al blob
        var blobClient = containerClient.GetBlobClient(uniqueFileName);

        // 5. Sube el archivo
        // Opcional: Configura el tipo de contenido (MIME type)
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "image/jpeg" }); 

        // 6. Devuelve la URL pública
        return blobClient.Uri.AbsoluteUri;
    }
}