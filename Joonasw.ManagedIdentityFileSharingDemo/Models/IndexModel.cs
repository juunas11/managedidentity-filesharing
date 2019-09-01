using Microsoft.AspNetCore.Http;

namespace Joonasw.ManagedIdentityFileSharingDemo.Models
{
    public class IndexModel
    {
        public IFormFile NewFile { get; set; }

        // TODO Add existing files
    }
}
