using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class LoginTests : BaseTest
{
    [Test]
    public void Login_With_Valid_Credentials_Should_Open_Account_Overview()
    {
        var loginPage = new LoginPage(Driver);
        var accountOverviewPage = new AccountOverviewPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        Assert.That(accountOverviewPage.IsLoaded(), Is.True);
    }
}
