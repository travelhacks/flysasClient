using AwardData;
using AwardWeb.Services;
using FlysasLib;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net.Http;

namespace AwardWeb
{
    public class Startup
    {
        string dbContextName = "AwardData";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddIdentity<ApplicationUser, IdentityRole>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireDigit = false;
                //config.Password.RequiredLength = 8;
            })
          .AddEntityFrameworkStores<AwardContext>()
          .AddDefaultTokenProviders();

            var connection = Configuration.GetConnectionString(dbContextName);
            var useInMemDb = connection.IsNullOrWhiteSpace();
            if (useInMemDb)
            {
                services.AddDbContext<AwardContext>(
                    options => options.UseInMemoryDatabase(dbContextName)
                );
                var provider = services.BuildServiceProvider();
                var ctx = provider.GetRequiredService<AwardContext>();
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                DbSeeder.Seed(ctx, userManager);
                var cachedData = new CachedData();
                var crawls = ctx.Crawls.Include(c => c.Route).ThenInclude(r => r.FromAirport).Include(c => c.Route).ThenInclude(r => r.ToAirport).ToList();
                cachedData.Set(crawls);
                services.AddSingleton<ICachedData>(cachedData);
            }
            else
            {
                services.AddLetsEncrypt();
                services.AddSingleton<ICachedData, CachedData>();
                services.AddDbContextPool<AwardContext>(
                    options => options.UseSqlServer(Configuration.GetConnectionString(dbContextName))
                );
                services.AddHostedService<HostedDataService>();
            }

            services.AddHttpClient<SASRestClient>().ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
            });
            services.Configure<Models.SMTPOptions>(Configuration.GetSection("SMTPOptions"), (BinderOptions o) => o.BindNonPublicProperties = true);
            services.Configure<Models.AppSettings>(Configuration.GetSection("AppSettings"), (BinderOptions o) => o.BindNonPublicProperties = true);


            services.AddTransient<IEmailSender, EmailSender>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var redirOptions = new Microsoft.AspNetCore.Rewrite.RewriteOptions();
            redirOptions.Rules.Add(new Code.RedirectToHttpsNoWWW());
            app.UseRewriter(redirOptions);

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                name: null,
                pattern: "Changes",
                defaults: new { controller = "Home", action = nameof(Controllers.HomeController.Changes) });

                routes.MapControllerRoute(
                    name: null,
                    pattern: "FAQ",
                    defaults: new { controller = "Home", action = nameof(Controllers.HomeController.FAQ) });

                routes.MapControllerRoute(
                  name: null,
                  pattern: "News",
                  defaults: new { controller = "Home", action = nameof(Controllers.HomeController.News) });

                routes.MapControllerRoute(
                    name: null,
                    pattern: "Alerts",
                    defaults: new { controller = "Alerts", action = nameof(Controllers.AlertsController.Index) });

                routes.MapControllerRoute(
                    name: null,
                    pattern: "List",
                    defaults: new { controller = "Home", action = nameof(Controllers.HomeController.List) });

                routes.MapControllerRoute(
                    name: null,
                    pattern: "Home/List",
                    defaults: new { controller = "Home", action = nameof(Controllers.HomeController.List_legacy) });

                routes.MapControllerRoute(
                   name: null,
                   pattern: "Console",
                   defaults: new { controller = "Home", action = nameof(Controllers.HomeController.Console) });

                routes.MapControllerRoute(
                   name: null,
                   pattern: "Home/Console",
                   defaults: new { controller = "Home", action = nameof(Controllers.HomeController.Console_legacy) });

                routes.MapControllerRoute(
                    name: null,
                    pattern: "Star",
                    defaults: new { controller = "Home", action = nameof(Controllers.HomeController.Star) });

                routes.MapControllerRoute(
                    name: null,
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
