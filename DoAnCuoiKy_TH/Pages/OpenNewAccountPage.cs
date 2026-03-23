using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy_TH.Pages;

public class OpenNewAccountPage
{
    private readonly IWebDriver driver;
    private readonly WebDriverWait wait;

    private static readonly By AccountTypeSelectLocator = By.Id("type");
    private static readonly By FromAccountSelectLocator = By.Id("fromAccountId");
    private static readonly By[] OpenNewAccountButtonLocators =
    [
        By.CssSelector("input[type='button'][value='Open New Account']"),
        By.CssSelector("input[type='submit'][value='Open New Account']"),
        By.CssSelector("button[type='submit']")
    ];
    private static readonly By NewAccountIdLocator = By.Id("newAccountId");
    private static readonly By OpenAccountResultLinkLocator = By.CssSelector("#openAccountResult a");
    private static readonly By OpenAccountHeaderLocator = By.XPath("//*[contains(text(),'Account Opened') or contains(text(),'Your new account number')]");

    public OpenNewAccountPage(IWebDriver driver)
    {
        this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void Open() => driver.FindElement(By.LinkText("Open New Account")).Click();

    public void SelectAccountType(string accountType)
    {
        var select = new SelectElement(driver.FindElement(AccountTypeSelectLocator));

        if (!string.IsNullOrWhiteSpace(accountType))
        {
            var option = select.Options.FirstOrDefault(o =>
                o.GetAttribute("value")?.Equals(accountType, StringComparison.OrdinalIgnoreCase) == true
                || o.Text.Contains(accountType, StringComparison.OrdinalIgnoreCase));

            if (option != null)
            {
                var value = option.GetAttribute("value") ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    select.SelectByValue(value);
                    return;
                }
            }
        }

        if (select.Options.Count > 0)
        {
            select.SelectByIndex(0);
        }
    }

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
                var value = option.GetAttribute("value") ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    select.SelectByValue(value);
                    return;
                }
            }
        }

        if (select.Options.Count > 0)
        {
            select.SelectByIndex(0);
        }
    }

    public void OpenAccount()
    {
        var button = OpenNewAccountButtonLocators
            .SelectMany(locator => driver.FindElements(locator))
            .FirstOrDefault();

        if (button == null)
        {
            throw new NoSuchElementException("Cannot find Open New Account submit button.");
        }

        button.Click();
    }

    public bool IsAccountCreated()
    {
        try
        {
            wait.Until(_ =>
                driver.Url.Contains("openaccount", StringComparison.OrdinalIgnoreCase)
                || driver.FindElements(NewAccountIdLocator).Any()
                || driver.FindElements(OpenAccountResultLinkLocator).Any()
                || driver.FindElements(OpenAccountHeaderLocator).Any());
        }
        catch (WebDriverTimeoutException)
        {
            // Ignore timeout and evaluate current page state.
        }

        var accountElement = driver.FindElements(NewAccountIdLocator).FirstOrDefault();
        if (accountElement != null && !string.IsNullOrWhiteSpace(accountElement.Text))
        {
            return true;
        }

        var resultLink = driver.FindElements(OpenAccountResultLinkLocator).FirstOrDefault();
        if (resultLink != null && !string.IsNullOrWhiteSpace(resultLink.Text))
        {
            return true;
        }

        return driver.FindElements(OpenAccountHeaderLocator).Any()
            || driver.Url.Contains("openaccount", StringComparison.OrdinalIgnoreCase);
    }
}
