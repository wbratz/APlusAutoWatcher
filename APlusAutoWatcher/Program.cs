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
        public static ILoggerFactory LoggerFactory;
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
            serviceCollection.AddSingleton<IGenerateChapterData, ChapterDataGenerator>();
            serviceCollection.AddSingleton<Application>();
        }

        private static void ConfigureLogging(IServiceCollection services)
        {
            services.AddLogging(opt =>
            {
                opt.AddConsole();
            });
        }

        public class Application
        {
            private readonly ILogger<Application> _logger;
            private readonly IScrapeWebpages _scraper;
            private readonly IGenerateChapterData _cdg;

            public Application(ILogger<Application> logger, IScrapeWebpages scraper, IGenerateChapterData cdg)
            {
                _logger = logger;
                _scraper = scraper;
                _cdg = cdg;
            }

            public void Run()
            {
                _logger.LogInformation("Starting Application..");

                var chapter = "8.1.1";

                while (chapter != "13.13.4")
                {
                    _logger.LogInformation($"Beginning {chapter}");
                    var nextSection = _scraper.ParseWebPage("v6_0_452", chapter, "7cf470c9-1e7d-484f-94c0-ade588ca139c");

                    _logger.LogInformation($"{chapter} Finished");
                    chapter = _cdg.GetNextChapter(chapter, nextSection);
                }

                _logger.LogInformation($"Final Chapter Complete");
            }
        }
    }
}