using Newtonsoft.Json;
using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class RegisterTests : BaseTest
{
    [Test]
    public void Register_With_Valid_Data_Should_Create_Account()
    {
        var testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "ParaBankAutomation", "TestData", "users.json");
        var users = JsonConvert.DeserializeObject<List<UserData>>(File.ReadAllText(testDataPath))
                    ?? throw new InvalidOperationException("No user data found in users.json");

        var registerPage = new RegisterPage(Driver);
        registerPage.Open(BaseUrl);
        registerPage.Register(users[0]);

        Assert.That(Driver.PageSource, Does.Contain("Your account was created successfully"));
    }
}
