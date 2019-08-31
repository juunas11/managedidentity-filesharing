using System;
using System.Security.Claims;

namespace Joonasw.ManagedIdentityFileSharingDemo.Extensions
{
    internal static class ClaimsPrincipalExtensions
    {
        public static string GetTenantId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string tenantId = user.FindFirstValue("tid"); //TODO not correct
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new Exception("Tenant id not found");
            }

            return tenantId;
        }

        public static bool IsPersonalAccount(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string tenantId = user.GetTenantId();
            // Personal MS accounts always have this tenant id
            // https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens#payload-claims
            return tenantId == "9188040d-6c67-4c5b-b112-36a304b66dad";
        }

        public static string GetObjectId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string objectId = user.FindFirstValue("oid"); //TODO not correct
            if (string.IsNullOrEmpty(objectId))
            {
                throw new Exception("Object id not found");
            }

            return objectId;
        }
    }
}
