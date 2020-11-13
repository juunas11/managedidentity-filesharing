using System.Collections.Generic;

namespace Joonasw.ManagedIdentityFileSharingDemo.Models
{
    public class SearchModel
    {
        public string Query { get; set; }
        public List<FileSearchResult> Results { get; set; }
    }
}
