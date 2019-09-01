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

        public async Task<string> GetAccessTokenAsync(string resource)
        {
            var tokenProvider = new AzureServiceTokenProvider();
            return await tokenProvider.GetAccessTokenAsync(resource, _tenantId);
        }

        public async Task<string> GetStorageAccessTokenAsync()
        {
            return await GetAccessTokenAsync("https://storage.azure.com/");
        }
    }
}
