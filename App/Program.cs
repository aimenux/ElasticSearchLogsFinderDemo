﻿using System;
using System.IO;
using System.Threading.Tasks;
using App.HostedServices;
using Lib.Configuration;
using Lib.ElasticSearch;
using Lib.Serializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "DEV";

            var host = new HostBuilder()
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.AddCommandLine(args);
                    hostConfig.AddEnvironmentVariables();
                    hostConfig.SetBasePath(Directory.GetCurrentDirectory());
                })
                .ConfigureAppConfiguration((hostingContext, appConfig) =>
                {
                    appConfig.AddCommandLine(args);
                    appConfig.AddEnvironmentVariables();
                    appConfig.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    appConfig.AddJsonFile("appsettings.json", true, true);
                    appConfig.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostingContext, services) =>
                {
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.AddNonGenericLogger();
                        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                        loggingBuilder.AddConsole(options =>
                        {
                            options.DisableColors = false;
                            options.TimestampFormat = "[HH:mm:ss:fff] ";
                        });
                        loggingBuilder.AddFile(hostingContext.Configuration.GetSection("Logging"));
                    });
                    services.AddLocalization();
                    services.AddHostedService<ElasticHostedService>();
                    services.AddTransient<IElasticProvider, ElasticProvider>();
                    services.AddTransient<ICustomJsonSerializer, CustomJsonSerializer>();
                    services.Configure<Settings>(hostingContext.Configuration.GetSection(nameof(Settings)));
                })
                .UseConsoleLifetime()
                .Build();

            await host.RunAsync();

            Console.WriteLine("End!");
            Console.WriteLine("Press any key to exit !");
            Console.ReadKey();
        }

        private static void AddNonGenericLogger(this ILoggingBuilder loggingBuilder)
        {
            var services = loggingBuilder.Services;
            services.AddSingleton(serviceProvider =>
            {
                const string categoryName = nameof(Program);
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger(categoryName);
            });
        }
    }
}
