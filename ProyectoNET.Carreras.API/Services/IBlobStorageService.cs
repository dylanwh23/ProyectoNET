public interface IBlobStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string containerName);
}