using OpenQA.Selenium;

namespace ParaBankAutomation.Pages;

public class RegisterPage
{
    private readonly IWebDriver driver;

    // Locators bạn đã cung cấp
    private static readonly By _firstNameInput = By.Id("customer.firstName");

    private static readonly By _lastNameInput = By.Id("customer.lastName");
    private static readonly By _addressInput = By.Id("customer.address.street");
    private static readonly By _cityInput = By.Id("customer.address.city");
    private static readonly By _stateInput = By.Id("customer.address.state");
    private static readonly By _zipCodeInput = By.Id("customer.address.zipCode");
    private static readonly By _phoneInput = By.Id("customer.phoneNumber");
    private static readonly By _ssnInput = By.Id("customer.ssn");
    private static readonly By _usernameInput = By.Id("customer.username");
    private static readonly By _passwordInput = By.Id("customer.password");
    private static readonly By _confirmPasswordInput = By.Id("repeatedPassword");
    private static readonly By _registerButton = By.CssSelector("input[value='Register']");

    // Locators hỗ trợ lấy kết quả (dành cho Parabank)
    private static readonly By _successMessage = By.CssSelector("#rightPanel p");

    private static readonly By _errorMessages = By.CssSelector(".error"); // Parabank thường dùng class .error cho các text màu đỏ

    public RegisterPage(IWebDriver driver)
    {
        this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    // --- Các hàm nhập liệu ---
    public void EnterFirstName(string text) => EnterText(_firstNameInput, text);

    public void EnterLastName(string text) => EnterText(_lastNameInput, text);

    public void EnterAddress(string text) => EnterText(_addressInput, text);

    public void EnterCity(string text) => EnterText(_cityInput, text);

    public void EnterState(string text) => EnterText(_stateInput, text);

    public void EnterZipCode(string text) => EnterText(_zipCodeInput, text);

    public void EnterPhone(string text) => EnterText(_phoneInput, text);

    public void EnterSSN(string text) => EnterText(_ssnInput, text);

    public void EnterUsername(string text) => EnterText(_usernameInput, text);

    public void EnterPassword(string text) => EnterText(_passwordInput, text);

    public void EnterConfirmPassword(string text) => EnterText(_confirmPasswordInput, text);

    // Hàm click đăng ký
    public void ClickRegister()
    {
        driver.FindElement(_registerButton).Click();
    }

    // --- Các hàm hỗ trợ lấy kết quả test ---
    public string GetSuccessMessage()
    {
        try { return driver.FindElement(_successMessage).Text; }
        catch (NoSuchElementException) { return string.Empty; }
    }

    public string GetErrorMessage()
    {
        try
        {
            // Có thể có nhiều lỗi hiển thị cùng lúc (ví dụ bỏ trống nhiều trường) Ta gộp tất cả lại
            // thành 1 chuỗi để dễ so sánh
            var errors = driver.FindElements(_errorMessages);
            if (errors.Count > 0)
            {
                return string.Join(" | ", errors.Select(e => e.Text).Where(t => !string.IsNullOrEmpty(t)));
            }
            return string.Empty;
        }
        catch (NoSuchElementException) { return string.Empty; }
    }

    // Hàm private dùng chung để tránh lặp code (DRY principle)
    private void EnterText(By locator, string text)
    {
        var element = driver.FindElement(locator);
        element.Clear();
        element.SendKeys(text ?? "");
    }
}