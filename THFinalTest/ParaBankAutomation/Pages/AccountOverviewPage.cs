using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class AccountOverviewPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _overviewTitle = By.XPath("//h1[contains(., 'Accounts Overview')]");

    public AccountOverviewPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public bool IsLoaded()
    {
        return _wait.WaitUntilVisible(_overviewTitle).Displayed;
    }
}
