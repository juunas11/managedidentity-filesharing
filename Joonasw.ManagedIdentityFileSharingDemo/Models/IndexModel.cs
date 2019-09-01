using Microsoft.AspNetCore.Http;

namespace Joonasw.ManagedIdentityFileSharingDemo.Models
{
    public class IndexModel
    {
        public IFormFile NewFile { get; set; }
        public FileModel[] Files { get; set; }
    }
}
