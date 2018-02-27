using System;
using System.Threading.Tasks;
using ApplicationModels;
using Common;
using Microsoft.EntityFrameworkCore;
using ApplicationModels.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Controllers;
using WebApp.Services;
#if NOAUTH
using Microsoft.AspNetCore.Mvc.Authorization;
#endif

namespace WebApp {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddResponseCompression();

            services.AddDbContext<ApplicationDbContext>();
            services.Configure<ApplicationDbContext>(options => { options.Database.SetCommandTimeout(5); });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            if (!(String.IsNullOrEmpty(Configuration["Authentication:Google:ClientId"]) ||
                  String.IsNullOrEmpty(Configuration["Authentication:Google:ClientSecret"]))) {
                services.AddAuthentication().AddGoogle(googleOptions => {
                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                });
            }

            services.Configure<IdentityOptions>(options => {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options => {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                // time after which asp net rejects cookie, causing logout
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.LoginPath = "/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
                options.Events.OnRedirectToLogin = context => {
                    if (context.Request.Path.StartsWithSegments("/api")) {
                        SetErrorResponseOnUnauthorizedCall(context);
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context => {
                    if (context.Request.Path.StartsWithSegments("/api")) {
                        SetErrorResponseOnUnauthorizedCall(context);
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddTransient<ContentDataController, ContentDataController>();
            services.AddScoped<IAccountManagementService, AccountManagementService>();
            #if MOCK
            services.AddScoped<IMarketingDataBackend, MockMarketingDataBackend>();
            services.AddScoped<IContentDataBackend, MockContentDataBackend>();
            #else
            services.AddScoped<IMarketingDataBackend, DBMarketingDataBackend>();
            services.AddScoped<IContentDataBackend, DBContentDataBackend>();
            #endif

            #if NOAUTH
            services.AddMvc(opts => {
                opts.Filters.Add(new AllowAnonymousFilter());
            });
            #else
            services.AddScoped<IDbInitializer, DbInitializer>();
            services.AddScoped<IEmailService, MailgunEmailService>();
            services.AddMvc();

            services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // enables immediate logout, after updating the user's stat.
                options.ValidationInterval = TimeSpan.Zero;
            });
            #endif
        }

        private void SetErrorResponseOnUnauthorizedCall(RedirectContext<CookieAuthenticationOptions> context) {
            context.Response.Clear();
            if (context.HttpContext.User.Identity.IsAuthenticated) {
                context.Response.StatusCode = 403;
            } else {
                context.Response.StatusCode = 401;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            } else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseResponseCompression();

            // System under a nginx proxy for HTTPS needs this
            var forwardedHeadersOptions = new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };
            forwardedHeadersOptions.KnownNetworks.Clear();
            forwardedHeadersOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardedHeadersOptions);

            app.UseAuthentication();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
