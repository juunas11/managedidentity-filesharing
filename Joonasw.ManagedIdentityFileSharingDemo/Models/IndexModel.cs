using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Joonasw.ManagedIdentityFileSharingDemo.Models
{
    public class IndexModel
    {
        [Required(ErrorMessage = "You must select a file")]
        public IFormFile NewFile { get; set; }
        public FileModel[] Files { get; set; }
    }
}
