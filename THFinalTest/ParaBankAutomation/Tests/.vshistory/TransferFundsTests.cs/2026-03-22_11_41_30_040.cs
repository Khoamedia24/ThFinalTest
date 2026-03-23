//using NUnit.Framework;
//using ParaBankAutomation.Base;
//using ParaBankAutomation.Pages;

//namespace ParaBankAutomation.Tests;

//public class TransferFundsTests : BaseTest
//{
//    [Test]
//    public void Transfer_Funds_Should_Show_Success_Message()
//    {
//        var loginPage = new LoginPage(Driver);
//        var transferFundsPage = new TransferFundsPage(Driver);

// loginPage.Open(BaseUrl); loginPage.Login("john", "demo");

// transferFundsPage.Open(BaseUrl); transferFundsPage.Transfer("10");

//        Assert.That(transferFundsPage.IsTransferSuccessful(), Is.True);
//    }
//}