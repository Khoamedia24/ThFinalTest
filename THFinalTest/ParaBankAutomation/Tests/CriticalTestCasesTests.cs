using OpenQA.Selenium;
using ParaBankAutomation.Base;
using ParaBankAutomation.Models;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class CriticalTestCasesTests : BaseTest
{
    [Test]
    public void TC_01_Login_With_Valid_Credentials_Should_Succeed()
    {
        var loginPage = new LoginPage(Driver);
        var accountOverviewPage = new AccountOverviewPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        Assert.That(accountOverviewPage.IsLoaded(), Is.True);
    }

    [Test]
    public void TC_02_Logout_Should_Return_To_Login_Page()
    {
        LoginAsJohn();

        Driver.FindElement(By.LinkText("Log Out")).Click();

        Assert.That(Driver.FindElements(By.CssSelector("input[type='submit'][value='Log In']")).Count, Is.GreaterThan(0));
    }

    [Test]
    public void TC_03_Transfer_Funds_Should_Show_Transfer_Complete()
    {
        LoginAsJohn();

        var transferFundsPage = new TransferFundsPage(Driver);
        transferFundsPage.Open(BaseUrl);
        var accounts = transferFundsPage.GetAvailableFromAccounts();
        var fromAccount = accounts[0];
        var toAccount = accounts.Count > 1 ? accounts[1] : accounts[0];

        transferFundsPage.Transfer("100", fromAccount, toAccount);

        Assert.That(transferFundsPage.IsTransferSuccessful(), Is.True);
    }

    [Test]
    public void TC_04_Account_Overview_Should_Load_With_Main_Columns()
    {
        LoginAsJohn();

        var accountOverviewPage = new AccountOverviewPage(Driver);
        Assert.That(accountOverviewPage.IsLoaded(), Is.True);
        Assert.That(Driver.PageSource, Does.Contain("Account"));
        Assert.That(Driver.PageSource, Does.Contain("Balance"));
        Assert.That(Driver.PageSource, Does.Contain("Available Amount"));
    }

    [Test]
    public void TC_14_Menu_Should_Be_Visible_Only_After_Login()
    {
        var loginPage = new LoginPage(Driver);
        loginPage.Open(BaseUrl);

        Assert.That(Driver.FindElements(By.LinkText("Transfer Funds")).Count, Is.EqualTo(0));

        loginPage.Login("john", "demo");

        Assert.That(Driver.FindElements(By.LinkText("Open New Account")).Count, Is.GreaterThan(0));
        Assert.That(Driver.FindElements(By.LinkText("Transfer Funds")).Count, Is.GreaterThan(0));
        Assert.That(Driver.FindElements(By.LinkText("Bill Pay")).Count, Is.GreaterThan(0));
        Assert.That(Driver.FindElements(By.LinkText("Find Transactions")).Count, Is.GreaterThan(0));
        Assert.That(Driver.FindElements(By.LinkText("Request Loan")).Count, Is.GreaterThan(0));
        Assert.That(Driver.FindElements(By.LinkText("Log Out")).Count, Is.GreaterThan(0));
    }

    [Test]
    public void TC_16_Register_With_Valid_Data_Should_Create_Account()
    {
        var registerPage = new RegisterPage(Driver);
        registerPage.Open(BaseUrl);

        var uniqueUser = BuildUser($"testuser_{DateTime.Now:yyyyMMddHHmmss}");
        registerPage.Register(uniqueUser);

        Assert.That(Driver.PageSource, Does.Contain("Your account was created successfully"));
    }

    [Test]
    public void TC_17_Register_With_Existing_Username_Should_Show_Error()
    {
        var registerPage = new RegisterPage(Driver);
        registerPage.Open(BaseUrl);

        var duplicateUser = BuildUser("john");
        registerPage.Register(duplicateUser);

        Assert.That(Driver.PageSource, Does.Contain("This username already exists"));
    }

    [Test]
    public void TC_25_Bill_Pay_With_Valid_Data_Should_Succeed()
    {
        LoginAsJohn();

        var billPayPage = new BillPayPage(Driver);
        billPayPage.Open(BaseUrl);
        billPayPage.PayBill("54321", "75");

        Assert.That(billPayPage.IsPaymentSuccessful(), Is.True);
    }

    [Test]
    public void TC_32_Request_Loan_With_Reasonable_Amount_Should_Be_Approved()
    {
        LoginAsJohn();

        var requestLoanPage = new RequestLoanPage(Driver);
        requestLoanPage.Open(BaseUrl);
        requestLoanPage.RequestLoan("1000", "100");

        Assert.That(requestLoanPage.IsResultVisible(), Is.True);
        Assert.That(Driver.PageSource, Does.Contain("Approved"));
    }

    [Test]
    public void TC_34_Update_Contact_Info_With_Valid_Data_Should_Succeed()
    {
        LoginAsJohn();

        var updateContactInfoPage = new UpdateContactInfoPage(Driver);
        updateContactInfoPage.Open(BaseUrl);
        updateContactInfoPage.UpdateContactInfo("Da Nang", "0236000222");

        Assert.That(updateContactInfoPage.IsUpdateSuccessful(), Is.True);
    }

    private void LoginAsJohn()
    {
        var loginPage = new LoginPage(Driver);
        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");
    }

    private static UserData BuildUser(string username)
    {
        return new UserData
        {
            FirstName = "Test",
            LastName = "User",
            Address = "123 Main St",
            City = "HCMC",
            State = "CA",
            ZipCode = "700000",
            Phone = "0909000111",
            Ssn = "123-45-6789",
            Username = username,
            Password = "Test@1234"
        };
    }
}
