using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    /// <summary>
    /// Adds an access token from Managed Identity to the DB connection when
    /// running in Azure.
    /// </summary>
    public class ManagedIdentityConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly string _tenantId;
        private readonly IWebHostEnvironment _environment;
        private readonly AzureServiceTokenProvider _tokenProvider;

        public ManagedIdentityConnectionInterceptor(
            AuthenticationOptions authenticationOptions,
            IWebHostEnvironment environment)
        {
            _tenantId = authenticationOptions.ManagedIdentityTenantId;
            if (string.IsNullOrEmpty(_tenantId))
            {
                _tenantId = null;
            }

            _environment = environment;
            _tokenProvider = new AzureServiceTokenProvider();
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

        private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            // Get access token for Azure SQL DB
            string resource = "https://database.windows.net/";
            return await _tokenProvider.GetAccessTokenAsync(resource, _tenantId, cancellationToken);
        }
    }
}
