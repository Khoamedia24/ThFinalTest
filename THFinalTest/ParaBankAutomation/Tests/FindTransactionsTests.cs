using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class FindTransactionsTests : BaseTest
{
    [Test]
    public void Find_Transactions_Page_Should_Load_Result_Area()
    {
        var loginPage = new LoginPage(Driver);
        var findTransactionsPage = new FindTransactionsPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        findTransactionsPage.Open(BaseUrl);
        Assert.That(findTransactionsPage.IsResultAreaVisible(), Is.True);
    }
}
