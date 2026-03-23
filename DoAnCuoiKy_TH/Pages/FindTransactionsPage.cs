using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy_TH.Pages;

public class FindTransactionsPage
{
    private readonly IWebDriver driver;
    private readonly WebDriverWait wait;

    private static readonly By[] TransactionIdLocators =
    [
        By.Id("criteria.transactionId"),
        By.Name("criteria.transactionId"),
        By.Id("transactionId"),
        By.Name("transactionId")
    ];

    private static readonly By[] TransactionDateLocators =
    [
        By.Id("criteria.onDate"),
        By.Name("criteria.onDate"),
        By.Id("onDate"),
        By.Name("onDate")
    ];

    private static readonly By[] TransactionFromDateLocators =
    [
        By.Id("criteria.fromDate"),
        By.Name("criteria.fromDate"),
        By.Id("fromDate"),
        By.Name("fromDate")
    ];

    private static readonly By[] TransactionToDateLocators =
    [
        By.Id("criteria.toDate"),
        By.Name("criteria.toDate"),
        By.Id("toDate"),
        By.Name("toDate")
    ];

    private static readonly By[] TransactionAmountLocators =
    [
        By.Id("criteria.amount"),
        By.Name("criteria.amount"),
        By.Id("amount"),
        By.Name("amount")
    ];

    private static readonly By[] FindTransactionButtonLocators =
    [
        By.CssSelector("#transactionForm button[type='submit']"),
        By.CssSelector("#transactionForm input[type='submit']"),
        By.CssSelector("input[value='Find Transactions']")
    ];

    private static readonly By TransactionRowsLocator = By.CssSelector("#transactionTable tbody tr");
    private static readonly By ErrorLocator = By.CssSelector("#rightPanel .error");

    public FindTransactionsPage(IWebDriver driver)
    {
        this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void Open()
    {
        driver.FindElement(By.LinkText("Find Transactions")).Click();

        wait.Until(_ =>
            driver.Url.Contains("findtrans", StringComparison.OrdinalIgnoreCase)
            || HasAnyElement(TransactionIdLocators)
            || HasAnyElement(TransactionDateLocators));
    }

    public void EnterTransactionId(string value) => EnterText(TransactionIdLocators, value);

    public void EnterTransactionDate(string value) => EnterText(TransactionDateLocators, value);

    public void EnterTransactionDateRange(string fromDate, string toDate)
    {
        EnterText(TransactionFromDateLocators, fromDate);
        EnterText(TransactionToDateLocators, toDate);
    }

    public void EnterAmount(string value) => EnterText(TransactionAmountLocators, value);

    public void Submit() => FindFirstElement(FindTransactionButtonLocators).Click();

    public bool HasValidationError()
    {
        try
        {
            return driver.FindElements(ErrorLocator).Any(e => !string.IsNullOrWhiteSpace(e.Text));
        }
        catch
        {
            return false;
        }
    }

    public bool HasTransactions()
    {
        try
        {
            return driver.FindElements(TransactionRowsLocator).Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private void EnterText(By[] locators, string value)
    {
        var input = FindFirstElement(locators);
        input.Clear();
        input.SendKeys(value);

        wait.Until(_ =>
        {
            var currentValue = input.GetAttribute("value") ?? string.Empty;
            if (string.IsNullOrEmpty(value))
            {
                return string.IsNullOrEmpty(currentValue);
            }

            if (!string.IsNullOrWhiteSpace(currentValue))
            {
                var normalizedCurrent = NormalizeDigitsOnly(currentValue);
                var normalizedExpected = NormalizeDigitsOnly(value);
                if (!string.IsNullOrWhiteSpace(normalizedCurrent) && !string.IsNullOrWhiteSpace(normalizedExpected))
                {
                    return normalizedCurrent.Contains(normalizedExpected, StringComparison.Ordinal)
                        || normalizedExpected.Contains(normalizedCurrent, StringComparison.Ordinal);
                }

                return true;
            }

            return false;
        });
    }

    private static string NormalizeDigitsOnly(string input)
    {
        return new string((input ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private IWebElement FindFirstElement(By[] locators)
    {
        return wait.Until(_ =>
        {
            foreach (var locator in locators)
            {
                var element = driver.FindElements(locator).FirstOrDefault();
                if (element != null)
                {
                    return element;
                }
            }

            return null;
        }) ?? throw new NoSuchElementException($"Cannot locate any of expected elements: {string.Join(", ", locators.Select(l => l.ToString()))}");
    }

    private bool HasAnyElement(By[] locators)
    {
        return locators.Any(locator => driver.FindElements(locator).Any());
    }
}
