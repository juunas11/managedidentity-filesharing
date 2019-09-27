using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Extensions;
using Joonasw.ManagedIdentityFileSharingDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class FileService
    {
        private const long MaxBytesPerUserOrOrg = 50 * 1024 * 1024; // 50 MB per user/organization
        private readonly AppDbContext _dbContext;
        private readonly AzureBlobStorageService _blobStorageService;

        public FileService(
            AppDbContext dbContext,
            AzureBlobStorageService blobStorageService)
        {
            _dbContext = dbContext;
            _blobStorageService = blobStorageService;
        }

        public async Task UploadFileAsync(IFormFile file, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            long fileSizeInBytes = file.Length;
            await CheckAvailableSpaceAsync(fileSizeInBytes, user, cancellationToken);

            Guid storedBlobId;
            using (Stream fileStream = file.OpenReadStream())
            {
                storedBlobId = await _blobStorageService.UploadBlobAsync(fileStream, user, cancellationToken);
            }

            var storedFile = new StoredFile
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                CreatorObjectId = user.GetObjectId(),
                CreatorTenantId = user.GetTenantId(),
                FileName = file.FileName,
                FileContentType = !string.IsNullOrEmpty(file.ContentType) ? file.ContentType : "application/octet-stream",
                StoredBlobId = storedBlobId,
                SizeInBytes = fileSizeInBytes
            };
            _dbContext.StoredFiles.Add(storedFile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task CheckAvailableSpaceAsync(long fileSizeInBytes, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            long storedBytes = await _dbContext.StoredFiles.ApplyAccessFilter(user).SumAsync(f => f.SizeInBytes, cancellationToken);
            if ((storedBytes + fileSizeInBytes) > MaxBytesPerUserOrOrg)
            {
                long maxMegaBytes = MaxBytesPerUserOrOrg / 1024 / 1024;
                throw new SpaceUnavailableException($"Sorry, max {maxMegaBytes} MB can be stored, delete files before uploading more");
            }
        }

        public async Task<(Stream stream, string fileName, string contentType)> DownloadFileAsync(
            Guid id, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            StoredFile file = await _dbContext.StoredFiles.SingleAsync(f => f.Id == id, cancellationToken);
            FileAccessUtils.CheckAccess(file, user);

            Stream stream = await _blobStorageService.DownloadBlobAsync(file.StoredBlobId, user, cancellationToken);
            return (stream, file.FileName, file.FileContentType);
        }

        public async Task<FileModel[]> GetFilesAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var files = _dbContext.StoredFiles.ApplyAccessFilter(user);

            return await files
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FileModel
                {
                    Id = f.Id,
                    Name = f.FileName,
                    CreatedAt = f.CreatedAt
                })
                .ToArrayAsync(cancellationToken);
        }

        public async Task DeleteFileAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            StoredFile file = await _dbContext.StoredFiles.SingleAsync(f => f.Id == id, cancellationToken);
            FileAccessUtils.CheckAccess(file, user);

            _dbContext.StoredFiles.Remove(file);

            // These two operations cannot be canceled, so no cancellation token
            await _blobStorageService.DeleteBlobAsync(file.StoredBlobId, user);

            await _dbContext.SaveChangesAsync();
        }
    }
}
