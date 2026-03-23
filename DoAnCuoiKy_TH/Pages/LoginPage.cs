using OpenQA.Selenium;

namespace DoAnCuoiKy_TH.Pages
{

    public class LoginPage
    {
        private readonly IWebDriver driver;
        private static readonly By UsernameInputLocator = By.Name("username");
        private static readonly By PasswordInputLocator = By.Name("password");
        private static readonly By LoginButtonLocator = By.CssSelector("input[type='submit'][value='Log In']");
        private static readonly By ErrorMessageLocator = By.CssSelector("#rightPanel .error");
        public LoginPage(IWebDriver driver)
        {
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }
        public void EnterUsername(string username)
        {
            var usernameInput = driver.FindElement(UsernameInputLocator);
            usernameInput.Clear();
            usernameInput.SendKeys(username);
        }
        public void EnterPassword(string password)
        {
            var passwordInput = driver.FindElement(PasswordInputLocator);
            passwordInput.Clear();
            passwordInput.SendKeys(password);
        }
        public void ClickLogin()
        {
            driver.FindElement(LoginButtonLocator).Click();
        }
        public string GetErrorMessage()
        {
            try
            {
                return driver.FindElement(ErrorMessageLocator).Text;
            }
            catch (NoSuchElementException)
            {
                return string.Empty;
            }
        }
    }
}
