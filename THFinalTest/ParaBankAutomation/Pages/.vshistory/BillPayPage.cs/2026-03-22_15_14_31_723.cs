using OpenQA.Selenium;

namespace THFinalTest.ParaBankAutomation.Pages
{
    public class BillPayPage
    {
        private readonly IWebDriver driver;

        // Các locators bạn đã cung cấp (Đã sửa lại _phone cho ổn định)
        private static readonly By _payeeName = By.CssSelector("input[name='payee.name']");

        private static readonly By _address = By.CssSelector("input[name='payee.address.street']");
        private static readonly By _city = By.CssSelector("input[name='payee.address.city']");
        private static readonly By _state = By.CssSelector("input[name='payee.address.state']");
        private static readonly By _zipCode = By.CssSelector("input[name='payee.address.zipCode']");
        private static readonly By _phone = By.CssSelector("input[name='payee.phoneNumber']"); // Đã sửa ID động
        private static readonly By _account = By.CssSelector("input[name='payee.accountNumber']");
        private static readonly By _verifyAccount = By.CssSelector("input[name='verifyAccount']");
        private static readonly By _amount = By.CssSelector("input[name='amount']");
        private static readonly By _sendPaymentButton = By.CssSelector("input[value='Send Payment']");

        // Locators cho phần thông báo lỗi/thành công
        private static readonly By _successMessage = By.CssSelector("#rightPanel .title"); // Thường ghi "Bill Payment Complete"

        private static readonly By _errorMessages = By.CssSelector(".error");

        public BillPayPage(IWebDriver driver)
        {
            this.driver = driver ?? throw new ArgumentNullException(nameof(driver));
        }

        // --- Các hàm nhập liệu ---
        public void EnterPayeeName(string text) => EnterText(_payeeName, text);

        public void EnterAddress(string text) => EnterText(_address, text);

        public void EnterCity(string text) => EnterText(_city, text);

        public void EnterState(string text) => EnterText(_state, text);

        public void EnterZipCode(string text) => EnterText(_zipCode, text);

        public void EnterPhone(string text) => EnterText(_phone, text);

        public void EnterAccount(string text) => EnterText(_account, text);

        public void EnterVerifyAccount(string text) => EnterText(_verifyAccount, text);

        public void EnterAmount(string text) => EnterText(_amount, text);

        public void ClickSendPayment()
        {
            driver.FindElement(_sendPaymentButton).Click();
        }

        // --- Các hàm hỗ trợ lấy kết quả ---
        public string GetSuccessMessage()
        {
            try { return driver.FindElement(_successMessage).Text; }
            catch (NoSuchElementException) { return string.Empty; }
        }

        public string GetErrorMessage()
        {
            try
            {
                var errors = driver.FindElements(_errorMessages);
                if (errors.Count > 0)
                {
                    return string.Join(" | ", errors.Select(e => e.Text).Where(t => !string.IsNullOrEmpty(t)));
                }
                return string.Empty;
            }
            catch (NoSuchElementException) { return string.Empty; }
        }

        private void EnterText(By locator, string text)
        {
            var element = driver.FindElement(locator);
            element.Clear();
            element.SendKeys(text ?? "");
        }
    }
}