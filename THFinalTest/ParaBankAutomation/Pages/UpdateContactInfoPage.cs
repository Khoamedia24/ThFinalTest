using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class UpdateContactInfoPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _firstNameInput = By.Id("customer.firstName");
    private readonly By _lastNameInput = By.Id("customer.lastName");
    private readonly By _addressInput = By.Id("customer.address.street");
    private readonly By _cityInput = By.Id("customer.address.city");
    private readonly By _stateInput = By.Id("customer.address.state");
    private readonly By _zipCodeInput = By.Id("customer.address.zipCode");
    private readonly By _phoneInput = By.Id("customer.phoneNumber");
    private readonly By _updateProfileButton = By.CssSelector("input[value='Update Profile']");

    public UpdateContactInfoPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/updateprofile.htm");
    }

    public void UpdateContactInfo(string city, string phone)
    {
        _wait.WaitUntilVisible(_firstNameInput);
        _driver.FindElement(_cityInput).Clear();
        _driver.FindElement(_cityInput).SendKeys(city);
        _driver.FindElement(_phoneInput).Clear();
        _driver.FindElement(_phoneInput).SendKeys(phone);
        _driver.FindElement(_updateProfileButton).Click();
    }

    public bool IsUpdateSuccessful()
    {
        return _driver.PageSource.Contains("Profile Updated", StringComparison.OrdinalIgnoreCase);
    }
}
