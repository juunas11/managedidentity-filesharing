using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class AzureBlobStorageService
    {
        private readonly StorageOptions _options;

        public AzureBlobStorageService(
            IOptions<StorageOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Uploads blob to tenant- / user-specific blob folder.
        /// </summary>
        /// <param name="content">File content</param>
        /// <param name="user">Current user</param>
        /// <returns>Generated blob id</returns>
        public async Task<Guid> UploadBlobAsync(Stream content, ClaimsPrincipal user)
        {
            var blobId = Guid.NewGuid();
            BlobClient client = GetBlobClient(blobId, user);
            await client.UploadAsync(content);
            return blobId;
        }

        /// <summary>
        /// Starts download of blob content.
        /// </summary>
        /// <param name="blobId">Id of stored blob returned by <see cref="UploadBlobAsync(Stream, ClaimsPrincipal)"/></param>
        /// <param name="user">Current user</param>
        /// <returns>Open stream to the blob in Storage</returns>
        public async Task<Stream> DownloadBlobAsync(Guid blobId, ClaimsPrincipal user)
        {
            BlobClient client = GetBlobClient(blobId, user);
            Response<BlobDownloadInfo> res = await client.DownloadAsync();
            return res.Value.Content;
        }

        /// <summary>
        /// Deletes a blob in Storage
        /// </summary>
        /// <param name="blobId">Id of stored blob returned by <see cref="UploadBlobAsync(Stream, ClaimsPrincipal)"/></param>
        /// <param name="user">Current user</param>
        public async Task DeleteBlobAsync(Guid blobId, ClaimsPrincipal user)
        {
            BlobClient client = GetBlobClient(blobId, user);
            await client.DeleteAsync();
        }

        private BlobClient GetBlobClient(Guid blobId, ClaimsPrincipal user)
        {
            // The blob folder is the tenant or user id
            // Personal accounts -> user id, organizational accounts -> tenant id
            string name = $"{FileAccessUtils.GetBlobFolder(user)}/{blobId}";

            BlobServiceClient client;

            if (_options.UseEmulator)
            {
                // Locally we use Storage Emulator
                client = new BlobServiceClient("UseDevelopmentStorage=true");
            }
            else
            {
                client = new BlobServiceClient(
                    new Uri($"https://{_options.AccountName}.blob.core.windows.net"),
                    new ManagedIdentityCredential());
            }

            BlobContainerClient containerClient = client.GetBlobContainerClient(_options.FileContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(name);
            return blobClient;
        }
    }
}
