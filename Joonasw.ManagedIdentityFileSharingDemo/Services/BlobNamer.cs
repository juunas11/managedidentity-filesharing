using Joonasw.ManagedIdentityFileSharingDemo.Extensions;
using System.Security.Claims;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class BlobNamer
    {
        public string GetBlobFolder(ClaimsPrincipal user)
        {
            // If user is personal MSA, folder is msa-{user-id}
            // If user is not personal, folder is org-{tenant-id}
            return user.IsPersonalAccount()
                ? $"msa-{user.GetObjectId()}"
                : $"org-{user.GetTenantId()}";
        }
    }
}
