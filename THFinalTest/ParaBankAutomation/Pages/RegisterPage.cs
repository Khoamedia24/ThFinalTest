using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class RegisterPage
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
    private readonly By _ssnInput = By.Id("customer.ssn");
    private readonly By _usernameInput = By.Id("customer.username");
    private readonly By _passwordInput = By.Id("customer.password");
    private readonly By _confirmPasswordInput = By.Id("repeatedPassword");
    private readonly By _registerButton = By.CssSelector("input[value='Register']");

    public RegisterPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/register.htm");
    }

    public void Register(UserData user)
    {
        _wait.WaitUntilVisible(_firstNameInput).SendKeys(user.FirstName);
        _driver.FindElement(_lastNameInput).SendKeys(user.LastName);
        _driver.FindElement(_addressInput).SendKeys(user.Address);
        _driver.FindElement(_cityInput).SendKeys(user.City);
        _driver.FindElement(_stateInput).SendKeys(user.State);
        _driver.FindElement(_zipCodeInput).SendKeys(user.ZipCode);
        _driver.FindElement(_phoneInput).SendKeys(user.Phone);
        _driver.FindElement(_ssnInput).SendKeys(user.Ssn);
        _driver.FindElement(_usernameInput).SendKeys(user.Username);
        _driver.FindElement(_passwordInput).SendKeys(user.Password);
        _driver.FindElement(_confirmPasswordInput).SendKeys(user.Password);
        _driver.FindElement(_registerButton).Click();
    }
}

public class UserData
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Ssn { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
