using Joonasw.ManagedIdentityFileSharingDemo.Extensions;
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
    public class BlobService
    {
        private readonly StorageOptions _options;
        private readonly AccessTokenFetcher _accessTokenFetcher;

        public BlobService(
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
            var blobName = Guid.NewGuid();
            CloudBlockBlob blob = await GetBlobReferenceAsync($"{GetBlobFolder(user)}/{blobName}");
            await blob.UploadFromStreamAsync(content);
            return blobName;
        }

        public async Task<Stream> DownloadBlobAsync(Guid blobName, ClaimsPrincipal user)
        {
            CloudBlockBlob blob = await GetBlobReferenceAsync($"{GetBlobFolder(user)}/{blobName}");
            return await blob.OpenReadAsync();
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

            string accessToken = await _accessTokenFetcher.GetAccessTokenAsync("https://storage.azure.com/");
            var tokenCredential = new TokenCredential(accessToken);
            var credentials = new StorageCredentials(tokenCredential);
            var uri = new Uri($"https://{_options.AccountName}.blob.core.windows.net/{_options.FileContainerName}/{name}");
            var blob = new CloudBlockBlob(uri, credentials);
            return blob;
        }

        private string GetBlobFolder(ClaimsPrincipal user)
        {
            // If user is personal MSA, folder is msa-{user-id}
            // If user is not personal, folder is org-{tenant-id}
            return user.IsPersonalAccount()
                ? $"msa-{user.GetObjectId()}"
                : $"org-{user.GetTenantId()}";
        }
    }
}
