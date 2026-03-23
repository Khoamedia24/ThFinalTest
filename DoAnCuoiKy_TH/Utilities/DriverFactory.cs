using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace DoAnCuoiKy_TH.Utilities
{
    /// <summary>
    /// Base class for test classes that provides WebDriver setup and teardown.
    /// </summary>
    public class DriverFactory
    {
        /// <summary>The WebDriver instance used for browser automation.</summary>
        protected IWebDriver driver = null!;

        /// <summary>The base URL for the application under test.</summary>
        private const string BaseUrl = "https://parabank.parasoft.com/parabank/index.htm";

        /// <summary>The implicit wait timeout in seconds.</summary>
        private const int ImplicitWaitSeconds = 5;

        /// <summary>
        /// Sets up the WebDriver before each test execution.
        /// Initializes the Edge browser with maximized window and navigates to the base URL.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var options = new EdgeOptions();
            options.AddArgument("start-maximized");

            driver = new EdgeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWaitSeconds);
            driver.Navigate().GoToUrl(BaseUrl);
        }

        /// <summary>
        /// Tears down the WebDriver after each test execution.
        /// Closes the browser and releases resources.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                }
                finally
                {
                    driver.Dispose();
                }
            }
        }
    }
}