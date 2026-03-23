using OpenQA.Selenium;
using ParaBankAutomation.Utilities;

namespace ParaBankAutomation.Pages;

public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WaitHelper _wait;

    private readonly By _usernameInput = By.Name("username");
    private readonly By _passwordInput = By.Name("password");
    private readonly By _loginButton = By.CssSelector("input[type='submit'][value='Log In']");

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WaitHelper(driver);
    }

    public void Open(string baseUrl)
    {
        _driver.Navigate().GoToUrl($"{baseUrl}/index.htm");
    }

    public void Login(string username, string password)
    {
        _wait.WaitUntilVisible(_usernameInput).SendKeys(username);
        _driver.FindElement(_passwordInput).SendKeys(password);
        _driver.FindElement(_loginButton).Click();
    }
}
