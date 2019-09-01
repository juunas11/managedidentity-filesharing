using System;
using System.Security.Claims;

namespace Joonasw.ManagedIdentityFileSharingDemo.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetTenantId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string tenantId = user.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
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
            return string.Equals(tenantId, "9188040d-6c67-4c5b-b112-36a304b66dad", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetObjectId(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string objectId = user.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
            if (string.IsNullOrEmpty(objectId))
            {
                throw new Exception("Object id not found");
            }

            return objectId;
        }

        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return user.FindFirstValue("name") ?? "Unknown";
        }

        /// <summary>
        /// Gets the username for display.
        /// Can be empty if claim not found.
        /// </summary>
        /// <param name="user">Current user</param>
        /// <returns>The username, e.g. user&amp;contoso.com, or empty if claim not found.</returns>
        public static string GetUserName(this ClaimsPrincipal user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return user.FindFirstValue("preferred_username") ?? "";
        }
    }
}
