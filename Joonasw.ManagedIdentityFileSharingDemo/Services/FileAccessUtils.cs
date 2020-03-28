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
                    throw new AccessDeniedException($"User object id {user.GetObjectId()} does not match file creator id {file.CreatorObjectId}");
                }
            }
            else
            {
                if (file.CreatorTenantId != user.GetTenantId())
                {
                    throw new AccessDeniedException($"User tenant id {user.GetTenantId()} does not match file tenant id {file.CreatorTenantId}");
                }
            }
        }

        internal static string GetBlobFolder(ClaimsPrincipal user)
        {
            // If a user uses a personal account, folder is msa-{user-id}
            // If a user uses an organizational AAD account, folder is org-{tenant-id}
            return user.IsPersonalAccount()
                ? $"msa-{user.GetObjectId()}"
                : $"org-{user.GetTenantId()}";
        }
    }
}
