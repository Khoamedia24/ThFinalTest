using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace ParaBankAutomation.Utilities;

public class WaitHelper
{
    private readonly WebDriverWait _wait;

    public WaitHelper(IWebDriver driver, int timeoutInSeconds = 10)
    {
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
    }

    public IWebElement WaitUntilVisible(By locator)
    {
        return _wait.Until(driver =>
        {
            var element = driver.FindElement(locator);
            return element.Displayed ? element : null;
        })!;
    }
}
