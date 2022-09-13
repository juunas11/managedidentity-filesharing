using Azure.Core;
using Azure.Identity;
using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Joonasw.ManagedIdentityFileSharingDemo.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;

namespace Joonasw.ManagedIdentityFileSharingDemo
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AddMvc(services);

            services.AddApplicationInsightsTelemetry(o =>
            {
                o.ConnectionString = _configuration["APPINSIGHTS_CONNECTIONSTRING"];
            });

            AddAuthentication(services);

            // Use DefaultAzureCredential to authenticate with Azure Storage and SQL DB
            var tokenCredential = new DefaultAzureCredential();
            AddDatabase(services, tokenCredential);
            AddStorage(services, tokenCredential);

            services.AddTransient<FileService>();
        }

        private static void AddMvc(IServiceCollection services)
        {
            // Cookie consent
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name = "FileSharing.CookieConsent";
            });

            // Core MVC services
            services.AddControllersWithViews(o =>
            {
                o.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
            });
        }

        private void AddAuthentication(IServiceCollection services)
        {
            var authenticationSettings = _configuration.GetSection("Authentication").Get<AuthenticationOptions>();
            // OpenID Connect authentication used to authenticate user
            // Cookie used to keep the authentication session on our side
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(o =>
                {
                    o.Authority = authenticationSettings.Authority;
                    o.ClientId = authenticationSettings.ClientId;
                    o.CallbackPath = "/aad-callback";
                    o.ResponseType = "id_token";
                    o.CorrelationCookie.IsEssential = true;
                    o.CorrelationCookie.Name = "FileSharing.Correlation";
                    o.Scope.Add("openid");
                    o.Scope.Add("profile");
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false
                    };
                }).AddCookie(o =>
                {
                    o.Cookie.IsEssential = true;
                    o.Cookie.Name = "FileSharing.Auth";
                    o.LoginPath = "/Account/Login";
                    o.AccessDeniedPath = "/Account/AccessDenied";
                });
        }

        private void AddDatabase(IServiceCollection services, TokenCredential tokenCredential)
        {
            // Setup the interceptor that will add access tokens to database connections in Azure
            services.AddDbContext<AppDbContext>(
                o => o.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Transient);
        }

        private void AddStorage(IServiceCollection services, TokenCredential tokenCredential)
        {
            services.Configure<StorageOptions>(_configuration.GetSection("Storage"));
            var options = _configuration.GetSection("Storage").Get<StorageOptions>();
            services.AddSingleton<AzureBlobStorageService>();

            services.AddAzureClients(builder =>
            {
                builder.UseCredential(tokenCredential);

                if (options.UseEmulator)
                {
                    builder.AddBlobServiceClient("UseDevelopmentStorage=true");
                }
                else
                {
                    builder.AddBlobServiceClient(new Uri($"https://{options.AccountName}.blob.core.windows.net"));
                }
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(o =>
            {
                o.MapControllers().RequireAuthorization();
            });
        }
    }
}
