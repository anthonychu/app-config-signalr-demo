﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.Models;
using Microsoft.Extensions.Logging;

namespace appconfigapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settings = config.Build();
                    var appConfigConnectionString = settings["ConnectionStrings:AppConfig"];
                    if (!string.IsNullOrEmpty(appConfigConnectionString)) {
                        config.AddAzureAppConfiguration(options =>
                            options.Connect(appConfigConnectionString)
                        .Watch("Settings:BackgroundColor", TimeSpan.FromSeconds(3)));
                    }
                })
                .UseStartup<Startup>();
    }
}
