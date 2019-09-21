using System;
using System.ComponentModel.DataAnnotations;

namespace Joonasw.ManagedIdentityFileSharingDemo.Data
{
    public class StoredFile
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(256)]
        public string FileName { get; set; }
        public Guid StoredBlobId { get; set; }
        [MaxLength(64)]
        public string CreatorTenantId { get; set; }
        [MaxLength(64)]
        public string CreatorObjectId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        [MaxLength(128)]
        public string FileContentType { get; set; }
        public long SizeInBytes { get; set; }
    }
}
