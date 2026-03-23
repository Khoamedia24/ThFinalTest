using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ParaBankAutomation.Utilities;

public static class DriverFactory
{
    public static IWebDriver CreateChromeDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");

        var runHeadless = Environment.GetEnvironmentVariable("HEADLESS")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
        if (runHeadless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        return new ChromeDriver(options);
    }
}
