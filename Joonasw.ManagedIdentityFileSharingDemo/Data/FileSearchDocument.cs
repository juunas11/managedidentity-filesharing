using System;
using System.Text.Json.Serialization;

namespace Joonasw.ManagedIdentityFileSharingDemo.Data
{
    internal class FileSearchDocument
    {
        [JsonPropertyName("metadata_storage_name")]
        public Guid BlobId { get; set; }
    }
}
