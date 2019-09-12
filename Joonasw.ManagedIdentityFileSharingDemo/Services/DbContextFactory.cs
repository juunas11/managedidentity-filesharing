using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Joonasw.ManagedIdentityFileSharingDemo.Services
{
    public class DbContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly AccessTokenFetcher _accessTokenFetcher;

        public DbContextFactory(
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment,
            AccessTokenFetcher accessTokenFetcher)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _accessTokenFetcher = accessTokenFetcher;
        }

        public async Task<AppDbContext> CreateContextAsync()
        {
            var context = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            bool useManagedIdentity = !_hostingEnvironment.IsDevelopment();
            if (!useManagedIdentity)
            {
                // In Development, we connect to a local SQL Server, no need for an access token
                return context;
            }

            // In Azure, get an access token and attach it to the connection
            string accessToken = await _accessTokenFetcher.GetSqlAccessTokenAsync();
            var conn = (SqlConnection)context.Database.GetDbConnection();
            conn.AccessToken = accessToken;
            return context;
        }
    }
}
