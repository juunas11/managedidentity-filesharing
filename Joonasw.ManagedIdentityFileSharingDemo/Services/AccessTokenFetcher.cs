using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class AccessTokenFetcher
    {
        private readonly string _tenantId;

        public AccessTokenFetcher(IOptions<AuthenticationOptions> options)
        {
            _tenantId = options.Value.ManagedIdentityTenantId;
            if (string.IsNullOrEmpty(_tenantId))
            {
                _tenantId = null;
            }
        }

        public async Task<string> GetSqlAccessTokenAsync()
        {
            return await GetAccessTokenAsync("https://database.windows.net/");
        }

        private async Task<string> GetAccessTokenAsync(string resource)
        {
            // This will return a cached token if one is there and valid
            // The token provider has a static in-memory cache
            var tokenProvider = new AzureServiceTokenProvider();
            return await tokenProvider.GetAccessTokenAsync(resource, _tenantId);
        }
    }
}
