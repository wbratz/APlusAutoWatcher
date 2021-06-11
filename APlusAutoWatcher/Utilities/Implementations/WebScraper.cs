using APlusAutoWatcher.Enums;
using APlusAutoWatcher.Utilities.Contracts;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Net.Http;
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

        public ChapterValues ParseWebPage(string version, string chapter, string uniqueId)
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

            var resourceOutline = WaitForLoad(moduleLoad);

            try
            {
                _driver.FindElementById("VideoViewer-playVideoLink").Click();
                _logger.LogInformation("Video found, playing video");
            }
            catch (NoSuchElementException ex)
            {
                _logger.LogError($"Could not find video player, assuming {chapter}, does not contain video", ex.Message);
            }

            //var pageSource = _driver.PageSource;

            //var pageDocument = new HtmlDocument();
            //pageDocument.LoadHtml(pageSource);

            var title = _driver.FindElementByClassName("ProductViewer-resourceTitle");

            Thread.Sleep(30000);
            _logger.LogInformation($"Waiting 30 seconds");

            if (IsEndOfSection(title))
            {
                _logger.LogInformation("Practice Test detected in {Chapter}", chapter);
                if (IsEndOfChapter(chapter, version, uniqueId))
                {
                    _logger.LogInformation("End of chapter reached");
                    return ChapterValues.Chapter;
                }

                _logger.LogInformation("End of section reached");
                return ChapterValues.Section;
            }

            _logger.LogInformation("End of subsection reached");
            return ChapterValues.Subsection;
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

        private bool IsEndOfSection(IWebElement element)
        {
            return element.Text.Contains("Practice Questions");
        }

        private bool IsEndOfChapter(string chapter, string version, string uniqueId)
        {
            var chapterArray = chapter.Split(".");

            var intValue = int.Parse(chapterArray[(int)ChapterValues.Section]);

            intValue++;

            chapterArray[(int)ChapterValues.Section] = intValue.ToString();

            _driver.Navigate()
                .GoToUrl($"https://labsimapp.testout.com/{version}/index.html/productviewer/242/{chapterArray[0]}.{chapterArray[1]}.{chapterArray[2]}/{uniqueId}");

            var moduleLoad = By.ClassName("ProductViewer-resourceTitle-outline");

            var secondHit = WaitForLoad(moduleLoad);

            return secondHit.Text.Equals("1.1");
        }
    }
}