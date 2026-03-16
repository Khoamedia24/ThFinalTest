using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class OpenNewAccountTests : BaseTest
{
    [Test]
    public void Open_New_Savings_Account_Should_Succeed()
    {
        var loginPage = new LoginPage(Driver);
        var openNewAccountPage = new OpenNewAccountPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        openNewAccountPage.Open(BaseUrl);
        openNewAccountPage.OpenNewAccount("SAVINGS");

        Assert.That(openNewAccountPage.IsAccountOpened(), Is.True);
    }
}
