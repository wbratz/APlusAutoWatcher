using APlusAutoWatcher.Utilities.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras;
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

        public string ParseWebPage(string baseUrl, string path, string path2)
        {
            var requesturi = $"{baseUrl}/{path}/{path2}";

            _driver.Navigate()
                .GoToUrl(requesturi);

            Login();

            var moduleLoad = By.ClassName("ProductViewer-resourceTitle-outline");

            while (path != _config.GetSection("EndPath").Value)
            {
                // To ensure javascript has fully loaded text into the element
                // Prevents receiving a blank element.
                var resourceOutline = WaitForVisible(moduleLoad);
                path = resourceOutline.Text;

                _logger.LogInformation($"Beginning {path}");

                PlayVideo();

                _logger.LogInformation($"Waiting 30 seconds");
                Thread.Sleep(30000);

                try
                {
                    MoveToNextModule();
                }
                catch (NoSuchElementException ex)
                {
                    _logger.LogError($"Could not find next button, exiting on error {ex.Message}");
                    break;
                }
            }

            return path;
        }

        private void MoveToNextModule()
        {
            var nextButton = _driver.FindElementById("ProductViewer-NavNextBtn");
            nextButton.Click();
        }

        private void PlayVideo()
        {
            // To ensure video player is fully loaded
            // Prevents unclickable link error
            var videoLinkElement = By.Id("VideoViewer-playVideoLink");
            try
            {
                WaitForClickable(videoLinkElement).Click();
            }
            catch (NullReferenceException)
            {
                _logger.LogInformation($"No video clink found, assuming no video on this page.");
                return;
            }

            _logger.LogInformation("Video found, playing video");
        }

        private void Login()
        {
            var login = By.Id("SignOn.login");
            var loginField = WaitForExist(login);

            if (loginField != null)
            {
                _logger.LogInformation($"Login field detected, logging in..");

                var usernameField = _driver.FindElementById("SignOn.login");
                var passwordField = _driver.FindElementById("SignOn.password");

                usernameField.SendKeys(_config.GetSection("UserName").Value);
                passwordField.SendKeys(_config.GetSection("Password").Value);

                _driver.FindElementById("SignOn.SignOnBtn").Click();

                _logger.LogInformation("Login Success!");
            }
        }

        private IWebElement WaitForExist(By element)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            try
            {
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(element));
            }
            catch (WebDriverTimeoutException nse)
            {
                _logger.LogError("Specified element is not found {Nse}, {Element}", nse.Message, element);
                return null;
            }
        }

        private IWebElement WaitForClickable(By element)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(4));
            try
            {
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(element));
            }
            catch (WebDriverTimeoutException nse)
            {
                _logger.LogError("Specified element is not found {Nse}, {Element}", nse.Message, element);
                return null;
            }
        }

        private IWebElement WaitForVisible(By element)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(4));
            try
            {
                return wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(element));
            }
            catch (WebDriverTimeoutException nse)
            {
                _logger.LogError("Specified element is not found {Nse}, {Element}", nse.Message, element);
                return null;
            }
        }
    }
}