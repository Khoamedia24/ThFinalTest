using NUnit.Framework;
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
        Driver.Quit();
        Driver.Dispose();
    }
}
