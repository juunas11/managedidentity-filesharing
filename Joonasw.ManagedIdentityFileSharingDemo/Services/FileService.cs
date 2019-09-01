using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class FileService
    {
        private readonly AppDbContext _dbContext;
        private readonly AzureBlobStorageService _blobStorageService;

        public FileService(
            AppDbContext dbContext,
            AzureBlobStorageService blobStorageService)
        {
            _dbContext = dbContext;
            _blobStorageService = blobStorageService;
        }

        public async Task UploadFileAsync(IFormFile file, ClaimsPrincipal user)
        {
            Guid storedBlobId;
            using (var fileStream = file.OpenReadStream())
            {
                storedBlobId = await _blobStorageService.UploadBlobAsync(fileStream, user);
            }

            var storedFile = new StoredFile
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatorObjectId = user.GetObjectId(),
                CreatorTenantId = user.GetTenantId(),
                FileName = file.FileName,
                StoredBlobId = storedBlobId
            };
            await _dbContext.StoredFiles.AddAsync(storedFile);
            await _dbContext.SaveChangesAsync();
        }
    }
}
