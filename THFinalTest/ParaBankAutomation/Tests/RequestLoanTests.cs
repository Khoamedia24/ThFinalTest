using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class RequestLoanTests : BaseTest
{
    [Test]
    public void Request_Loan_Should_Return_Result()
    {
        var loginPage = new LoginPage(Driver);
        var requestLoanPage = new RequestLoanPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        requestLoanPage.Open(BaseUrl);
        requestLoanPage.RequestLoan("100", "10");

        Assert.That(requestLoanPage.IsResultVisible(), Is.True);
    }
}
