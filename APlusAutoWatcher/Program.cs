using APlusAutoWatcher.Data;
using APlusAutoWatcher.Utilities.Contracts;
using APlusAutoWatcher.Utilities.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
            if (environment == "Development")
            {
                builder
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory,
                        string.Format("..{0}..{0}..{0}",
                        Path.DirectorySeparatorChar),
                        $"appsettings.{environment}.json"),
                        optional: true
                    );
            }
            else
            {
                builder
                    .AddJsonFile($"appsettings.{environment}.json", optional: false);
            }

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

            public Application(ILogger<Application> logger, IScrapeWebpages scraper)
            {
                _logger = logger;
                _scraper = scraper;
            }

            public void Run()
            {
                _logger.LogInformation("Starting Application..");

                var chapter = "6.1.1";

                _logger.LogInformation($"Beginning {chapter}");
                var exitChapter = _scraper.ParseWebPage("v6_0_452", chapter, "7cf470c9-1e7d-484f-94c0-ade588ca139c");

                _logger.LogInformation($"Exited on {exitChapter}.");

                if (exitChapter == "13.13.4")
                {
                    _logger.LogInformation($"Final Chapter Complete");
                }
                else
                {
                    _logger.LogInformation($"Program exited prematurely, check logs for error \r Last chapter read: {exitChapter}");
                }
            }
        }
    }
}