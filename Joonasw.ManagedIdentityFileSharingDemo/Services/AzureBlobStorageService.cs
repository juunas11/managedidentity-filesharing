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
            CloudBlockBlob blob = await GetBlobReferenceAsync($"{FileAccessUtils.GetBlobFolder(user)}/{blobId}");
            await blob.UploadFromStreamAsync(content);
            return blobId;
        }

        public async Task<Stream> DownloadBlobAsync(Guid blobId, ClaimsPrincipal user)
        {
            CloudBlockBlob blob = await GetBlobReferenceAsync($"{FileAccessUtils.GetBlobFolder(user)}/{blobId}");
            return await blob.OpenReadAsync();
        }

        public async Task DeleteBlobAsync(Guid blobId, ClaimsPrincipal user)
        {
            CloudBlockBlob blob = await GetBlobReferenceAsync($"{FileAccessUtils.GetBlobFolder(user)}/{blobId}");
            await blob.DeleteAsync();
        }

        private async Task<CloudBlockBlob> GetBlobReferenceAsync(string name)
        {
            if (_options.UseEmulator)
            {
                var account = CloudStorageAccount.DevelopmentStorageAccount;
                var blobClient = account.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_options.FileContainerName);
                return container.GetBlockBlobReference(name);
            }

            string accessToken = await _accessTokenFetcher.GetStorageAccessTokenAsync();
            var tokenCredential = new TokenCredential(accessToken);
            var credentials = new StorageCredentials(tokenCredential);
            var uri = new Uri($"https://{_options.AccountName}.blob.core.windows.net/{_options.FileContainerName}/{name}");
            var blob = new CloudBlockBlob(uri, credentials);
            return blob;
        }
    }
}
