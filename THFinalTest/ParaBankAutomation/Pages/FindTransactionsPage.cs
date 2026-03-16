using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class FindTransactionsPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _transactionIdInput = By.Id("transactionId");
    private readonly By _findByIdButton = By.XPath("//button[@type='submit' and contains(., 'Find Transaction')]");
    private readonly By _resultsContainer = By.Id("transactionForm");

    public FindTransactionsPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/findtrans.htm");
    }

    public void FindByTransactionId(string transactionId)
    {
        _wait.WaitUntilVisible(_transactionIdInput).SendKeys(transactionId);
        _driver.FindElement(_findByIdButton).Click();
    }

    public bool IsResultAreaVisible()
    {
        return _wait.WaitUntilVisible(_resultsContainer).Displayed;
    }
}
