using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Extensions;
using Joonasw.ManagedIdentityFileSharingDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class FileService
    {
        private readonly DbContextFactory _dbContextFactory;
        private readonly AzureBlobStorageService _blobStorageService;

        public FileService(
            DbContextFactory dbContextFactory,
            AzureBlobStorageService blobStorageService)
        {
            _dbContextFactory = dbContextFactory;
            _blobStorageService = blobStorageService;
        }

        public async Task UploadFileAsync(IFormFile file, ClaimsPrincipal user)
        {
            var dbContext = await _dbContextFactory.CreateContextAsync();
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
                FileContentType = !string.IsNullOrEmpty(file.ContentType) ? file.ContentType : "application/octet-stream",
                StoredBlobId = storedBlobId
            };
            await dbContext.StoredFiles.AddAsync(storedFile);
            await dbContext.SaveChangesAsync();
        }

        public async Task<(Stream stream, string fileName, string contentType)> DownloadFileAsync(Guid id, ClaimsPrincipal user)
        {
            var dbContext = await _dbContextFactory.CreateContextAsync();
            var file = await dbContext.StoredFiles.SingleAsync(f => f.Id == id);
            if (user.IsPersonalAccount())
            {
                if (file.CreatorObjectId != user.GetObjectId())
                {
                    throw new AccessDeniedException();
                }
            }
            else
            {
                if (file.CreatorTenantId != user.GetTenantId())
                {
                    throw new AccessDeniedException();
                }
            }

            var stream = await _blobStorageService.DownloadBlobAsync(file.StoredBlobId, user);
            return (stream, file.FileName, file.FileContentType);
        }

        public async Task<FileModel[]> GetFilesAsync(ClaimsPrincipal user)
        {
            var dbContext = await _dbContextFactory.CreateContextAsync();
            IQueryable<StoredFile> files = dbContext.StoredFiles;
            if (user.IsPersonalAccount())
            {
                files = files.Where(f => f.CreatorObjectId == user.GetObjectId());
            }
            else
            {
                files = files.Where(f => f.CreatorTenantId == user.GetTenantId());
            }

            return await files
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FileModel
                {
                    Id = f.Id,
                    Name = f.FileName,
                    CreatedAt = f.CreatedAt
                })
                .ToArrayAsync();
        }
    }
}
