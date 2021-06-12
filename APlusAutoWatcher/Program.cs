using APlusAutoWatcher.Utilities.Contracts;
using APlusAutoWatcher.Utilities.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace APlusAutoWatcher
{
    internal class Program
    {
        public static IConfigurationRoot Configuration;

        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            /*
             * Use service provider var when service is needed in Program.cs *
             *
             * Example:
             * var packageFileService = serviceProvider.GetRequiredService<IPackageFileService>();
            */

            IConfigurationBuilder builder = ConfigureBuild();
            Configuration = builder.Build();

            ConfigureLogging(serviceCollection);
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var app = serviceProvider.GetService<Application>();

            app.Run();
        }

        private static IConfigurationBuilder ConfigureBuild()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");

            Console.WriteLine("Environment: {0}", environment);

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", optional: true);

            return builder;
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IConfiguration>(Configuration);
            serviceCollection.AddSingleton<IScrapeWebpages, WebScraper>();
            serviceCollection.AddSingleton<Application>();
        }

        private static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(opt =>
            {
                opt.ClearProviders();
                opt.AddConsole();
            });
        }

        public class Application
        {
            private readonly ILogger<Application> _logger;
            private readonly IScrapeWebpages _scraper;
            private readonly IConfiguration _config;

            public Application(ILogger<Application> logger, IScrapeWebpages scraper, IConfiguration config)
            {
                _logger = logger;
                _scraper = scraper;
                _config = config;
            }

            public void Run()
            {
                _logger.LogInformation("Starting Application..");

                var baseUrl = _config.GetSection("BaseUrl").Value;
                var path = _config.GetSection("Path").Value;
                var path2 = _config.GetSection("Path2").Value;

                _logger.LogInformation($"Program starting on {path}");

                var exitPath = _scraper.ParseWebPage(baseUrl, path, path2);

                _logger.LogInformation($"Program Exited on {exitPath}.");

                if (exitPath == _config.GetSection("EndPath").Value)
                {
                    _logger.LogInformation($"Final Chapter Complete");
                }
                else
                {
                    _logger.LogInformation($"Program exited prematurely, check logs for error \r Last chapter read: {exitPath}");
                }
            }
        }
    }
}