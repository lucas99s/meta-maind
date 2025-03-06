using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Backend.Services
{
    public class AzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureStorageService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureStorage:ConnectionString"]
                ?? throw new ArgumentNullException("ConnectionString não configurada");

            _containerName = configuration["AzureStorage:ContainerName"]
                ?? throw new ArgumentNullException("ContainerName não configurado");

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public BlobContainerClient GetContainerClient()
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName);
        }

        public async Task UploadFileAsync(string localFilePath, Dictionary<string, string> metadata)
        {
            var containerClient = GetContainerClient();
            string fileName = Path.GetFileName(localFilePath);
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(localFilePath, overwrite: true);

            // Adicionar metadados personalizados
            await blobClient.SetMetadataAsync(metadata);
        }

        public async Task UploadStreamAsync(string fileName, Stream fileStream, string contentType, Dictionary<string, string> metadata)
        {
            var containerClient = GetContainerClient();
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

            // Adicionar metadados personalizados
            await blobClient.SetMetadataAsync(metadata);
        }
    }
}
