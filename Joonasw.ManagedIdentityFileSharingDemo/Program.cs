using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Joonasw.ManagedIdentityFileSharingDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    var config = configBuilder.Build();
                    var keyVaultUrl = config["KeyVault:Url"];
                    if (!string.IsNullOrEmpty(keyVaultUrl))
                    {
                        configBuilder.AddAzureKeyVault(keyVaultUrl);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
