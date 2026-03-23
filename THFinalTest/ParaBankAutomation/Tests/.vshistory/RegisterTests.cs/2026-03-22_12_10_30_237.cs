using DoAnCuoiKy_TH.DataProviders;
using OpenQA.Selenium;
using ParaBankAutomation.Pages;
using ParaBankAutomation.Utilities;
using System.Globalization;
using THFinalTest.ParaBankAutomation.Models;

namespace ParaBankAutomation.Tests;

[TestFixture]
public class RegisterTests : DriverFactory
{
    #region Constants

    private const string ExcelSheetName = "TestCase_Tam";
    private const string TestCasePrefixFilter = "TC_";

    // NH? ??I S? NÀY THEO ?ÚNG STT C?A CÁC CASE REGISTER TRONG EXCEL C?A B?N NHÉ
    private const int MinEnabledTestCaseNumber = 1;

    private const int MaxEnabledTestCaseNumber = 10;

    #endregion Constants

    private RegisterPage? registerPage;

    [TestCaseSource(nameof(GetVisibleRegisterTestCases))]
    public void ExecuteRegisterTestCase(string testCaseId, string function, string bigItem)
    {
        if (!ShouldRunTestCase(testCaseId, out _))
        {
            Assert.Ignore($"Skipping {testCaseId}. Only {TestCasePrefixFilter}{MinEnabledTestCaseNumber:00} to {TestCasePrefixFilter}{MaxEnabledTestCaseNumber:00} are enabled.");
        }

        var testCase = ExcelDataProvider.GetTestCaseById(ExcelSheetName, TestCasePrefixFilter, testCaseId);
        registerPage = new RegisterPage(driver);

        LogTestCaseInfo(testCaseId, function, bigItem);

        string actualResult;
        bool isTestPassed;
        string notes = "";

        try
        {
            // M? trang ??ng ký tr??c khi ch?y các step (N?u b?n có step "M? trang register" trong
            // Excel thì ?o?n này có th? ?? trong ExecuteTestStep) driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");

            foreach (var step in testCase.Steps)
            {
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                ExecuteTestStep(step);
            }

            actualResult = GetActualResult();
            isTestPassed = IsResultMatchingExpected(testCase.Expected, actualResult);
        }
        catch (Exception ex)
        {
            actualResult = ex.Message;
            isTestPassed = false;
        }

        if (!isTestPassed)
        {
            string screenshotPath = CaptureScreenshot(testCaseId);
            notes = $"Screenshot: {screenshotPath}";
            TestContext.Out.WriteLine($"Screenshot saved at: {screenshotPath}");
        }

        string resultStatus = isTestPassed ? "PASS" : "FAIL";
        string updatedExcelRange = ExcelDataProvider.WriteTestResults(testCase.Steps, actualResult, resultStatus, ExcelSheetName, notes);

        LogTestResults(testCase.Expected, actualResult, resultStatus, updatedExcelRange);

        Assert.That(isTestPassed, Is.True, $"Test case {testCaseId} failed. Expected: {testCase.Expected}, Actual: {actualResult}");
    }

    // --- Các hàm l?y Test Cases (Gi? nguyên nh? bên Login) ---
    private static IEnumerable<object[]> GetVisibleRegisterTestCases()
    {
        foreach (var testCaseData in ExcelDataProvider.GetNamedTestCases(ExcelSheetName, TestCasePrefixFilter))
        {
            if (testCaseData.Length > 0 && testCaseData[0] is string testCaseId && ShouldRunTestCase(testCaseId, out _))
            {
                yield return testCaseData;
            }
        }
    }

    private static bool ShouldRunTestCase(string testCaseId, out int caseNumber)
    {
        caseNumber = 0;
        if (string.IsNullOrWhiteSpace(testCaseId) || !testCaseId.StartsWith(TestCasePrefixFilter, StringComparison.OrdinalIgnoreCase)) return false;
        var numericPart = testCaseId[TestCasePrefixFilter.Length..];
        if (!int.TryParse(numericPart, NumberStyles.None, CultureInfo.InvariantCulture, out caseNumber)) return false;
        return caseNumber >= MinEnabledTestCaseNumber && caseNumber <= MaxEnabledTestCaseNumber;
    }

    // --- B? THÔNG D?CH STEP ACTION ---
    private void ExecuteTestStep(TestStep step)
    {
        var action = step.StepAction?.ToLower() ?? "";
        var testData = step.TestData ?? "";

        if (registerPage == null) throw new InvalidOperationException("RegisterPage is not initialized");

        // B?t t? khóa t? c?t Step action trong Excel
        if (action.Contains("m? trang") || action.Contains("open"))
        {
            driver.Navigate().GoToUrl(testData); // L?y link t? c?t test data
        }
        else if (action.Contains("first name")) registerPage.EnterFirstName(testData);
        else if (action.Contains("last name")) registerPage.EnterLastName(testData);
        else if (action.Contains("address")) registerPage.EnterAddress(testData);
        else if (action.Contains("city")) registerPage.EnterCity(testData);
        else if (action.Contains("state")) registerPage.EnterState(testData);
        else if (action.Contains("zip code")) registerPage.EnterZipCode(testData);
        else if (action.Contains("phone")) registerPage.EnterPhone(testData);
        else if (action.Contains("ssn")) registerPage.EnterSSN(testData);
        else if (action.Contains("username")) registerPage.EnterUsername(testData);
        else if (action.Contains("confirm")) registerPage.EnterConfirmPassword(testData); // Ph?i ?? lên trên password ?? không b? trùng t? khóa
        else if (action.Contains("password")) registerPage.EnterPassword(testData);
        else if (action.Contains("register") || action.Contains("??ng ký"))
        {
            registerPage.ClickRegister();
        }
    }

    // --- HÀM L?Y K?T QU? ---
    private string GetActualResult()
    {
        if (registerPage == null) throw new InvalidOperationException("RegisterPage is not initialized");

        try
        {
            // Ki?m tra xem có xu?t hi?n l?i ?? nào trên màn hình không
            string errorMessage = registerPage.GetErrorMessage();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage; // Tr? v? text l?i (VD: "Username already exists")
            }

            // N?u không có l?i, th? l?y câu thông báo thành công
            string successMessage = registerPage.GetSuccessMessage();
            if (successMessage.Contains("created") || successMessage.Contains("successfully"))
            {
                return "??ng ký thành công";
            }

            // N?u trang chuy?n h??ng th?ng v? overview
            if (driver.Url.Contains("overview"))
            {
                return "??ng ký thành công";
            }

            return "Không xác ??nh ???c tr?ng thái";
        }
        catch (Exception ex)
        {
            return $"L?i h? th?ng: {ex.Message}";
        }
    }

    // Các hàm ph? tr? gi? nguyên y h?t bên LoginTests
    private static bool IsResultMatchingExpected(string expectedText, string actualText)
    {
        if (string.IsNullOrWhiteSpace(expectedText)) return false;
        var expected = NormalizeText(expectedText);
        var actual = NormalizeText(actualText);
        return actual.Contains(expected) || expected.Contains(actual) || actual.Equals(expected);
    }

    private static string NormalizeText(string input) => (input ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").ToLowerInvariant().Trim();

    private string CaptureScreenshot(string testCaseId)
    {
        try
        {
            string screenshotDirectory = Path.Combine(AppContext.BaseDirectory, "Screenshots");
            if (!Directory.Exists(screenshotDirectory)) Directory.CreateDirectory(screenshotDirectory);
            string fileName = $"{testCaseId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string screenshotPath = Path.Combine(screenshotDirectory, fileName);
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(screenshotPath);
            return screenshotPath;
        }
        catch (Exception ex) { return $"Failed to capture screenshot: {ex.Message}"; }
    }

    private void LogTestCaseInfo(string testCaseId, string function, string bigItem)
    { /* Gi? nguyên log */ }

    private void LogTestResults(string expected, string actual, string status, string updatedRange)
    { /* Gi? nguyên log */ }
}