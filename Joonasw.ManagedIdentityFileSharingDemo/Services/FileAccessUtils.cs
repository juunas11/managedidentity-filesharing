using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Extensions;
using System.Linq;
using System.Security.Claims;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    internal static class FileAccessUtils
    {
        internal static IQueryable<StoredFile> ApplyAccessFilter(
            this IQueryable<StoredFile> files,
            ClaimsPrincipal user)
        {
            if (user.IsPersonalAccount())
            {
                return files.Where(f => f.CreatorObjectId == user.GetObjectId());
            }

            return files.Where(f => f.CreatorTenantId == user.GetTenantId());
        }

        internal static void CheckAccess(StoredFile file, ClaimsPrincipal user)
        {
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
        }

        internal static string GetBlobFolder(ClaimsPrincipal user)
        {
            // If user is personal MSA, folder is msa-{user-id}
            // If user is not personal, folder is org-{tenant-id}
            return user.IsPersonalAccount()
                ? $"msa-{user.GetObjectId()}"
                : $"org-{user.GetTenantId()}";
        }
    }
}
