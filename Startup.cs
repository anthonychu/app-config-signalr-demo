using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using appconfigapp.Data;
using appconfigapp.Models;
using appconfigapp.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace appconfigapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(Configuration.GetSection("Settings"));

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var isSqlite = connectionString.Contains("datasource=", StringComparison.InvariantCultureIgnoreCase);
            if (isSqlite) {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));
            } else {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddSignalR();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IOptionsMonitor<Settings> optMonitor,
            IHubContext<SettingsHub> settingsHubContext,
            ApplicationDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            optMonitor.OnChange(settings => {
                System.Console.WriteLine("***** changed!");
                settingsHubContext.Clients.All.SendAsync("backgroundChanged", settings.BackgroundColor).Wait();
            });

            app.UseSignalR(builder => {
                builder.MapHub<SettingsHub>("/settingshub");
            });

            app.UseMvc();

            dbContext.Database.Migrate();
        }
    }
}
