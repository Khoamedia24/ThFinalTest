using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy_TH.Pages;

public class RequestLoanPage
{
    private readonly IWebDriver driver;

    private static readonly By LoanAmountInputLocator = By.Id("amount");
    private static readonly By DownPaymentInputLocator = By.Id("downPayment");
    private static readonly By FromAccountSelectLocator = By.Id("fromAccountId");
    private static readonly By ApplyNowButtonLocator = By.CssSelector("input[type='button'][value='Apply Now']");
    private static readonly By ApprovedMessageLocator = By.Id("loanRequestApproved");
    private static readonly By DeniedMessageLocator = By.Id("loanRequestDenied");
    private static readonly By ErrorLocator = By.CssSelector("#requestLoanForm .error");

    public RequestLoanPage(IWebDriver driver)
    {
        this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    public void Open() => driver.FindElement(By.LinkText("Request Loan")).Click();

    public void EnterLoanAmount(string value) => EnterText(LoanAmountInputLocator, value);

    public void EnterDownPayment(string value) => EnterText(DownPaymentInputLocator, value);

    public void SelectFromAccount(string accountHint)
    {
        var select = new SelectElement(driver.FindElement(FromAccountSelectLocator));

        if (!string.IsNullOrWhiteSpace(accountHint))
        {
            var option = select.Options.FirstOrDefault(o =>
                o.GetAttribute("value")?.Equals(accountHint, StringComparison.OrdinalIgnoreCase) == true
                || o.Text.Contains(accountHint, StringComparison.OrdinalIgnoreCase));

            if (option != null)
            {
                select.SelectByValue(option.GetAttribute("value") ?? string.Empty);
                return;
            }
        }

        if (select.Options.Count > 0)
        {
            select.SelectByIndex(0);
        }
    }

    public void ApplyNow() => driver.FindElement(ApplyNowButtonLocator).Click();

    public bool IsApproved() => driver.FindElements(ApprovedMessageLocator).Any();

    public bool IsDenied() => driver.FindElements(DeniedMessageLocator).Any();

    public bool HasValidationError()
    {
        return driver.FindElements(ErrorLocator).Any(e => !string.IsNullOrWhiteSpace(e.Text));
    }

    private void EnterText(By locator, string value)
    {
        var input = driver.FindElement(locator);
        input.Clear();
        input.SendKeys(value);
    }
}
