using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Data
{
    /// <summary>
    /// Adds an access token from Managed Identity to the DB connection when
    /// running in Azure.
    /// </summary>
    public class ManagedIdentityConnectionInterceptor : DbConnectionInterceptor
    {
        private static readonly TimeSpan RefreshTokenBeforeExpiry = TimeSpan.FromMinutes(10);
        private readonly IWebHostEnvironment _environment;
        private readonly TokenCredential _tokenCredential;
        private AccessToken _cachedToken;

        public ManagedIdentityConnectionInterceptor(
            IWebHostEnvironment environment,
            TokenCredential tokenCredential)
        {
            _environment = environment;
            _tokenCredential = tokenCredential;
        }

        public override async Task<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            bool useManagedIdentity = !_environment.IsDevelopment();

            if (useManagedIdentity)
            {
                // In Azure, get an access token for the connection
                var sqlConnection = (SqlConnection)connection;
                string accessToken = await GetAccessTokenAsync(cancellationToken);
                sqlConnection.AccessToken = accessToken;
            }

            return result;
        }

        private async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (CachedTokenValid())
            {
                return _cachedToken.Token;
            }

            // Get access token for Azure SQL DB
            string scope = "https://database.windows.net/.default";
            var token = await _tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { scope }), cancellationToken);
            _cachedToken = token;
            return token.Token;
        }

        private bool CachedTokenValid()
        {
            return DateTimeOffset.UtcNow + RefreshTokenBeforeExpiry < _cachedToken.ExpiresOn;
        }
    }
}
