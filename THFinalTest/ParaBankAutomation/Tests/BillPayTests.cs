using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class BillPayTests : BaseTest
{
    [Test]
    public void Bill_Payment_Should_Show_Completed_Message()
    {
        var loginPage = new LoginPage(Driver);
        var billPayPage = new BillPayPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        billPayPage.Open(BaseUrl);
        billPayPage.PayBill("123456", "10");

        Assert.That(billPayPage.IsPaymentSuccessful(), Is.True);
    }
}
