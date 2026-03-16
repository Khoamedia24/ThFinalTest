using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class TransferFundsPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _amountInput = By.Id("amount");
    private readonly By _fromAccountSelect = By.Id("fromAccountId");
    private readonly By _toAccountSelect = By.Id("toAccountId");
    private readonly By _transferButton = By.CssSelector("input[value='Transfer']");
    private readonly By _successMessage = By.XPath("//h1[contains(., 'Transfer Complete!')]");

    public TransferFundsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/transfer.htm");
    }

    public void Transfer(string amount)
    {
        _wait.WaitUntilVisible(_amountInput).SendKeys(amount);

        var fromAccount = new SelectElement(_driver.FindElement(_fromAccountSelect));
        var toAccount = new SelectElement(_driver.FindElement(_toAccountSelect));

        fromAccount.SelectByIndex(0);
        toAccount.SelectByIndex(0);

        _driver.FindElement(_transferButton).Click();
    }

    public bool IsTransferSuccessful()
    {
        return _wait.WaitUntilVisible(_successMessage).Displayed;
    }
}
