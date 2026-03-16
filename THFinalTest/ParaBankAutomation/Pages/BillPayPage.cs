using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class BillPayPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _payeeNameInput = By.Name("payee.name");
    private readonly By _addressInput = By.Name("payee.address.street");
    private readonly By _cityInput = By.Name("payee.address.city");
    private readonly By _stateInput = By.Name("payee.address.state");
    private readonly By _zipCodeInput = By.Name("payee.address.zipCode");
    private readonly By _phoneInput = By.Name("payee.phoneNumber");
    private readonly By _accountInput = By.Name("payee.accountNumber");
    private readonly By _verifyAccountInput = By.Name("verifyAccount");
    private readonly By _amountInput = By.Name("amount");
    private readonly By _sendPaymentButton = By.CssSelector("input[value='Send Payment']");
    private readonly By _successMessage = By.XPath("//h1[contains(., 'Bill Payment Complete')]");

    public BillPayPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/billpay.htm");
    }

    public void PayBill(string accountNumber, string amount)
    {
        _wait.WaitUntilVisible(_payeeNameInput).SendKeys("Electric Company");
        _driver.FindElement(_addressInput).SendKeys("123 Main St");
        _driver.FindElement(_cityInput).SendKeys("HCM");
        _driver.FindElement(_stateInput).SendKeys("HCM");
        _driver.FindElement(_zipCodeInput).SendKeys("700000");
        _driver.FindElement(_phoneInput).SendKeys("0900000000");
        _driver.FindElement(_accountInput).SendKeys(accountNumber);
        _driver.FindElement(_verifyAccountInput).SendKeys(accountNumber);
        _driver.FindElement(_amountInput).SendKeys(amount);
        _driver.FindElement(_sendPaymentButton).Click();
    }

    public bool IsPaymentSuccessful()
    {
        return _wait.WaitUntilVisible(_successMessage).Displayed;
    }
}
