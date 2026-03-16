using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class RequestLoanPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _loanAmountInput = By.Id("amount");
    private readonly By _downPaymentInput = By.Id("downPayment");
    private readonly By _applyNowButton = By.CssSelector("input[value='Apply Now']");
    private readonly By _resultTitle = By.XPath("//h1[contains(., 'Loan Request Processed') or contains(., 'Loan Request')] ");

    public RequestLoanPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/requestloan.htm");
    }

    public void RequestLoan(string amount, string downPayment)
    {
        _wait.WaitUntilVisible(_loanAmountInput).SendKeys(amount);
        _driver.FindElement(_downPaymentInput).SendKeys(downPayment);
        _driver.FindElement(_applyNowButton).Click();
    }

    public bool IsResultVisible()
    {
        return _wait.WaitUntilVisible(_resultTitle).Displayed;
    }
}
