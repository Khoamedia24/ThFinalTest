using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Base;

public abstract class BaseTest
{
    protected IWebDriver Driver = null!;
    protected string BaseUrl = "https://parabank.parasoft.com/parabank";

    [SetUp]
    public void SetUp()
    {
        Driver = DriverFactory.CreateChromeDriver();
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Driver != null && TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
            {
                if (Driver is ITakesScreenshot screenshotTaker)
                {
                    var screenshot = screenshotTaker.GetScreenshot();
                    var directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Screenshots");
                    Directory.CreateDirectory(directory);

                    var fileName = $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var filePath = Path.Combine(directory, fileName);
                    screenshot.SaveAsFile(filePath);
                    TestContext.AddTestAttachment(filePath);
                }
            }
        }
        finally
        {
            Driver?.Quit();
            Driver?.Dispose();
        }
    }
}
