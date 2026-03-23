using OpenQA.Selenium;

namespace THFinalTest.ParaBankAutomation.Pages
{
    public class BillPayPage
    {
        private static readonly By _payeeName = By.CssSelector("input[name='payee.name']");
        private static readonly By _address = By.CssSelector("input[name='payee.address.street']");
        private static readonly By _city = By.CssSelector("input[name='payee.address.city']");
        private static readonly By _state = By.CssSelector("input[name='payee.address.state']");
        private static readonly By _zipCode = By.CssSelector("input[name='payee.address.zipCode']");
        private static readonly By _phone = By.CssSelector("input[id='995f9239-4788-4de8-ae8a-5c760bb64ace']");
        private static readonly By _account = By.CssSelector("input[name='payee.accountNumber']");
        private static readonly By _verifyAccount = By.CssSelector("input[name='verifyAccount']");
        private static readonly By _amount = By.CssSelector("input[name='amount']");
        private static readonly By _sendPayment = By.CssSelector("input[value='Send Payment']");
    }
}