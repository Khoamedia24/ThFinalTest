using NUnit.Framework;
using ParaBankAutomation.Base;
using ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests;

public class UpdateContactInfoTests : BaseTest
{
    [Test]
    public void Update_Contact_Info_Should_Show_Success_Message()
    {
        var loginPage = new LoginPage(Driver);
        var updateContactInfoPage = new UpdateContactInfoPage(Driver);

        loginPage.Open(BaseUrl);
        loginPage.Login("john", "demo");

        updateContactInfoPage.Open(BaseUrl);
        updateContactInfoPage.UpdateContactInfo("Ho Chi Minh", "0912345678");

        Assert.That(updateContactInfoPage.IsUpdateSuccessful(), Is.True);
    }
}
