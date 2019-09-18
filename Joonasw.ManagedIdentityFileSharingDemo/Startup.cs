using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Joonasw.ManagedIdentityFileSharingDemo.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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

            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            var authenticationSettings = _configuration.GetSection("Authentication").Get<AuthenticationOptions>();
            AddAuthentication(services, authenticationSettings);

            AddDatabase(services, authenticationSettings);
            AddStorage(services);

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
            }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
        }

        private void AddAuthentication(IServiceCollection services, AuthenticationOptions authenticationSettings)
        {
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

        private void AddDatabase(IServiceCollection services, AuthenticationOptions authenticationSettings)
        {
            // Setup the interceptor that will add access tokens to database connections in Azure
            var managedIdentityInterceptor = new ManagedIdentityConnectionInterceptor(
                authenticationSettings,
                _environment);
            services.AddDbContext<AppDbContext>(
                o => o.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")).AddInterceptors(managedIdentityInterceptor),
                ServiceLifetime.Transient);
        }

        private void AddStorage(IServiceCollection services)
        {
            services.Configure<StorageOptions>(_configuration.GetSection("Storage"));
            services.AddSingleton<AzureBlobStorageService>();
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
