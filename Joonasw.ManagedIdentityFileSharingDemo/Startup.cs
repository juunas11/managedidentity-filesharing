using Joonasw.ManagedIdentityFileSharingDemo.Data;
using Joonasw.ManagedIdentityFileSharingDemo.Options;
using Joonasw.ManagedIdentityFileSharingDemo.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Joonasw.ManagedIdentityFileSharingDemo
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.ConsentCookie.Name = "FileSharing.CookieConsent";
            });

            services.AddMvc(o =>
            {
                o.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
                o.Filters.Add(new AuthorizeFilter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddApplicationInsightsTelemetry(_configuration["APPINSIGHTS_INSTRUMENTATIONKEY"]);

            AddAuthentication(services);
            services.Configure<AuthenticationOptions>(_configuration.GetSection("Authentication"));
            services.Configure<StorageOptions>(_configuration.GetSection("Storage"));
            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")), ServiceLifetime.Transient);
            services.AddSingleton<AccessTokenFetcher>();
            services.AddSingleton<DbContextFactory>();
            services.AddSingleton<AzureBlobStorageService>();
            services.AddSingleton<FileService>();
            services.AddHttpContextAccessor();
        }

        private void AddAuthentication(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(o =>
                {
                    var opts = _configuration.GetSection("Authentication").Get<AuthenticationOptions>();
                    o.Authority = opts.Authority;
                    o.ClientId = opts.ClientId;
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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
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

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
