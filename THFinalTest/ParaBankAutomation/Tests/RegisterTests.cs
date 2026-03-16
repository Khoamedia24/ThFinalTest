using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Data;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class RegisterTests : BaseTest
{
    [Test]
    public void Register_With_Valid_Data_Should_Create_Account()
    {
        var testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ParaBankAutomation", "TestData", "users.xlsx");
        var users = ExcelUserDataReader.ReadUsers(testDataPath);

        var registerPage = new RegisterPage(Driver);
        registerPage.Open(BaseUrl);
        registerPage.Register(users[0]);

        Assert.That(Driver.PageSource, Does.Contain("Your account was created successfully"));
    }
}
