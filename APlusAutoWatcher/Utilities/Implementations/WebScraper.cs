using APlusAutoWatcher.Utilities.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace APlusAutoWatcher.Utilities.Implementations
{
    public class WebScraper : IScrapeWebpages
    {
        private readonly IConfiguration _config;
        private readonly ChromeDriver _driver;
        private readonly ILogger<IScrapeWebpages> _logger;

        public WebScraper(IConfiguration config, ILogger<IScrapeWebpages> logger)
        {
            _config = config;
            _logger = logger;

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var options = new ChromeOptions();

            _driver = new ChromeDriver(path, options);

            _logger.LogInformation("Web Scraper Initialized");
        }

        public string ParseWebPage(string version, string chapter, string uniqueId)
        {
            var requesturi = $"https://labsimapp.testout.com/{version}/index.html/productviewer/242/{chapter}/{uniqueId}";

            _driver.Navigate()
                .GoToUrl(requesturi);

            var login = By.Id("SignOn.login");
            var loginField = WaitForLoad(login);

            if (loginField != null)
            {
                _logger.LogInformation($"Login field detected, logging in with supplied information");

                var usernameField = _driver.FindElementById("SignOn.login");
                var passwordField = _driver.FindElementById("SignOn.password");

                usernameField.SendKeys(_config.GetSection("UserName").Value);
                passwordField.SendKeys(_config.GetSection("Password").Value);

                _driver.FindElementById("SignOn.SignOnBtn").Click();

                _logger.LogInformation("Login Success");
            }

            var moduleLoad = By.ClassName("ProductViewer-resourceTitle-outline");

            while (chapter != "13.13.4")
            {
                var resourceOutline = WaitForLoad(moduleLoad);

                // To ensure javascript has fully loaded text into the element
                // Prevents receiving a blank element.
                Thread.Sleep(5000);
                chapter = resourceOutline.Text;

                try
                {
                    // To ensure video player is fully loaded
                    // Prevents unclickable link error
                    Thread.Sleep(5000);

                    _driver.FindElementById("VideoViewer-playVideoLink").Click();
                    _logger.LogInformation("Video found, playing video");
                }
                catch (NoSuchElementException ex)
                {
                    _logger.LogError($"Could not find video player, assuming {resourceOutline.Text} does not contain video \r Error Message: {ex.Message}");
                }

                var title = _driver.FindElementByClassName("ProductViewer-resourceTitle");

                Thread.Sleep(30000);
                _logger.LogInformation($"Waiting 30 seconds");

                try
                {
                    var nextButton = _driver.FindElementById("ProductViewer-NavNextBtn");
                    nextButton.Click();
                }
                catch (NoSuchElementException ex)
                {
                    _logger.LogError($"Could not find next button, exiting on error {ex.Message}");
                    break;
                }
            }

            return chapter;
        }

        private IWebElement WaitForLoad(By element)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            try
            {
                return wait.Until(ExpectedConditions.ElementExists(element));
            }
            catch (WebDriverTimeoutException nse)
            {
                _logger.LogError("Specified element is not found {Nse}, {Element}", nse.Message, element);
                return null;
            }
        }
    }
}