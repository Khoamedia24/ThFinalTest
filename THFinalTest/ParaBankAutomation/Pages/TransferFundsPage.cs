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
        var accounts = GetAvailableFromAccounts();
        var fromAccount = accounts[0];
        var toAccount = accounts.Count > 1 ? accounts[1] : accounts[0];

        Transfer(amount, fromAccount, toAccount);
    }

    public void Transfer(string amount, string fromAccount, string toAccount)
    {
        var amountElement = _wait.WaitUntilVisible(_amountInput);
        amountElement.Clear();
        amountElement.SendKeys(amount);

        WaitForAccountOptions();

        var fromSelect = new SelectElement(_driver.FindElement(_fromAccountSelect));
        var toSelect = new SelectElement(_driver.FindElement(_toAccountSelect));

        SelectAccount(fromSelect, fromAccount);
        SelectAccount(toSelect, toAccount);

        _driver.FindElement(_transferButton).Click();
    }

    public List<string> GetAvailableFromAccounts()
    {
        WaitForAccountOptions();
        var fromSelect = new SelectElement(_driver.FindElement(_fromAccountSelect));
        return fromSelect.Options
            .Select(option => option.GetAttribute("value") ?? option.Text)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
    }

    public bool IsTransferSuccessful()
    {
        return _wait.WaitUntilVisible(_successMessage).Displayed;
    }

    private void WaitForAccountOptions()
    {
        _wait.WaitUntil(driver =>
        {
            var fromSelect = new SelectElement(driver.FindElement(_fromAccountSelect));
            var toSelect = new SelectElement(driver.FindElement(_toAccountSelect));
            return fromSelect.Options.Count > 0 && toSelect.Options.Count > 0;
        });
    }

    private static void SelectAccount(SelectElement selectElement, string account)
    {
        if (selectElement.Options.Any(o => (o.GetAttribute("value") ?? string.Empty) == account))
        {
            selectElement.SelectByValue(account);
            return;
        }

        selectElement.SelectByText(account);
    }
}
