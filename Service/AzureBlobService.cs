using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using PictureAnalyzer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PictureAnalyzer.Service
{
    public class AzureBlobService
    {
        private IConfiguration _configuration;

        public AzureBlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        async public Task<BlobContainerClient> GetContainer()
        {
            var container = new BlobContainerClient(_configuration.GetConnectionString("AzureStorageConnectionString"), _configuration["AzueStorageContainerName"]);
            var createResponse = await container.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);

            return container;
        }

        async public Task DeleteBlob(string blobId)
        {
            var container = await GetContainer();
            await container.DeleteBlobIfExistsAsync(blobId, Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);

        }

        async public Task<Common.SasUri> GetUploadSasForNewBlob()
        {
            var container = await GetContainer();

            var blobId = Guid.NewGuid().ToString();
            var blob = container.GetBlobClient(blobId); //model.FileName

            // If a blob with the same name exists, then we delete the Blob and its snapshots.
            await blob.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);

            BlobSasBuilder sasBuilder = new BlobSasBuilder();
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Write);

            var sasUri = blob.GenerateSasUri(sasBuilder);

            return new Common.SasUri()
            {
                Sas = sasUri,
                BlobId = blobId
            };
        }

        async public Task<Common.SasUri> GetViewSasForBlob(string blobId)
        {
            var container = await GetContainer();

            var blob = container.GetBlobClient(blobId); //model.FileName

            BlobSasBuilder sasBuilder = new BlobSasBuilder();
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            var sasUri = blob.GenerateSasUri(sasBuilder);

            return new Common.SasUri()
            {
                Sas = sasUri,
                BlobId = blobId
            };
        }
    }
}
