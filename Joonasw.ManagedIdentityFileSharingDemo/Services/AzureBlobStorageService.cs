using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class AzureBlobStorageService
    {
        private readonly StorageOptions _options;
        private readonly AccessTokenFetcher _accessTokenFetcher;

        public AzureBlobStorageService(
            IOptions<StorageOptions> options,
            AccessTokenFetcher accessTokenFetcher)
        {
            _options = options.Value;
            _accessTokenFetcher = accessTokenFetcher;
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
            CloudBlockBlob blob = await GetBlobReferenceAsync(blobId, user);
            await blob.UploadFromStreamAsync(content);
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
            CloudBlockBlob blob = await GetBlobReferenceAsync(blobId, user);
            return await blob.OpenReadAsync();
        }

        /// <summary>
        /// Deletes a blob in Storage
        /// </summary>
        /// <param name="blobId">Id of stored blob returned by <see cref="UploadBlobAsync(Stream, ClaimsPrincipal)"/></param>
        /// <param name="user">Current user</param>
        public async Task DeleteBlobAsync(Guid blobId, ClaimsPrincipal user)
        {
            CloudBlockBlob blob = await GetBlobReferenceAsync(blobId, user);
            await blob.DeleteAsync();
        }

        private async Task<CloudBlockBlob> GetBlobReferenceAsync(Guid blobId, ClaimsPrincipal user)
        {
            // The blob folder is the tenant or user id
            // Personal accounts -> user id, organizational accounts -> tenant id
            string name = $"{FileAccessUtils.GetBlobFolder(user)}/{blobId}";

            if (_options.UseEmulator)
            {
                // Locally we use Storage Emulator, we have to interact with it a bit differently
                var account = CloudStorageAccount.DevelopmentStorageAccount;
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_options.FileContainerName);
                return container.GetBlockBlobReference(name);
            }

            // In Azure, acquire access token and use that
            string accessToken = await _accessTokenFetcher.GetStorageAccessTokenAsync();
            var tokenCredential = new TokenCredential(accessToken);
            var credentials = new StorageCredentials(tokenCredential);
            var uri = new Uri($"https://{_options.AccountName}.blob.core.windows.net/{_options.FileContainerName}/{name}");
            return new CloudBlockBlob(uri, credentials);
        }
    }
}
