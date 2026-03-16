using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class OpenNewAccountPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _accountTypeSelect = By.Id("type");
    private readonly By _fromAccountSelect = By.Id("fromAccountId");
    private readonly By _openNewAccountButton = By.CssSelector("input[value='Open New Account']");
    private readonly By _successTitle = By.XPath("//h1[contains(., 'Account Opened!')]");

    public OpenNewAccountPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/openaccount.htm");
    }

    public void OpenNewAccount(string accountType)
    {
        var accountTypeSelect = new SelectElement(_wait.WaitUntilVisible(_accountTypeSelect));
        var fromAccountSelect = new SelectElement(_driver.FindElement(_fromAccountSelect));

        accountTypeSelect.SelectByText(accountType);
        fromAccountSelect.SelectByIndex(0);
        _driver.FindElement(_openNewAccountButton).Click();
    }

    public bool IsAccountOpened()
    {
        return _wait.WaitUntilVisible(_successTitle).Displayed;
    }
}
