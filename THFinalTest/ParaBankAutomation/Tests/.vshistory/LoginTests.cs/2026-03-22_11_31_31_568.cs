using DoAnCuoiKy_TH.DataProviders;
using OpenQA.Selenium;
using ParaBankAutomation.Pages;
using ParaBankAutomation.Utilities;
using System.Globalization;
using THFinalTest.ParaBankAutomation.Models;

namespace ParaBankAutomation.Tests;

[TestFixture]
public class LoginTests : DriverFactory
{
    #region Constants

    private const string ExcelSheetName = "TestCase_Tam";
    private const string TestCasePrefixFilter = "TC_";
    private const int MinEnabledTestCaseNumber = 1;
    private const int MaxEnabledTestCaseNumber = 5;

    #endregion Constants

    private LoginPage? loginPage;

    [TestCaseSource(nameof(GetVisibleLoginTestCases))]
    public void ExecuteLoginTestCase(string testCaseId, string function, string bigItem)
    {
        if (!ShouldRunTestCase(testCaseId, out _))
        {
            Assert.Ignore($"Skipping {testCaseId}. Only {TestCasePrefixFilter}{MinEnabledTestCaseNumber:00} to {TestCasePrefixFilter}{MaxEnabledTestCaseNumber:00} are enabled.");
        }

        var testCase = ExcelDataProvider.GetTestCaseById(ExcelSheetName, TestCasePrefixFilter, testCaseId);
        loginPage = new LoginPage(driver);

        LogTestCaseInfo(testCaseId, function, bigItem);

        string actualResult;
        bool isTestPassed;
        string notes = "";

        try
        {
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

    private static IEnumerable<object[]> GetVisibleLoginTestCases()
    {
        foreach (var testCaseData in ExcelDataProvider.GetNamedTestCases(ExcelSheetName, TestCasePrefixFilter))
        {
            if (testCaseData.Length == 0 || testCaseData[0] is not string testCaseId)
            {
                continue;
            }

            if (ShouldRunTestCase(testCaseId, out _))
            {
                yield return testCaseData;
            }
        }
    }

    private static bool ShouldRunTestCase(string testCaseId, out int caseNumber)
    {
        caseNumber = 0;

        if (string.IsNullOrWhiteSpace(testCaseId) || !testCaseId.StartsWith(TestCasePrefixFilter, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var numericPart = testCaseId[TestCasePrefixFilter.Length..];

        if (!int.TryParse(numericPart, NumberStyles.None, CultureInfo.InvariantCulture, out caseNumber))
        {
            return false;
        }

        return caseNumber >= MinEnabledTestCaseNumber && caseNumber <= MaxEnabledTestCaseNumber;
    }

    private void ExecuteTestStep(TestStep step)
    {
        var action = step.StepAction?.ToLower() ?? "";
        var testData = step.TestData ?? "";

        if (loginPage == null)
        {
            throw new InvalidOperationException("LoginPage is not initialized");
        }

        if (action.Contains("username"))
        {
            loginPage.EnterUsername(testData);
        }
        else if (action.Contains("password"))
        {
            loginPage.EnterPassword(testData);
        }
        else if (action.Contains("login") || action.Contains("click"))
        {
            loginPage.ClickLogin();
        }
    }

    private string GetActualResult()
    {
        if (loginPage == null)
        {
            throw new InvalidOperationException("LoginPage is not initialized");
        }

        try
        {
            string currentUrl = driver.Url;

            if (currentUrl.Contains("overview"))
            {
                return "Đăng nhập thành công";
            }

            string errorMessage = loginPage.GetErrorMessage();

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            if (currentUrl.Contains("index.htm") || currentUrl.Contains("login.htm"))
            {
                return "Login failed";
            }

            return "Login successful";
        }
        catch (Exception ex)
        {
            return $"Error checking result: {ex.Message}";
        }
    }

    private static bool IsResultMatchingExpected(string expectedText, string actualText)
    {
        if (string.IsNullOrWhiteSpace(expectedText))
        {
            return false;
        }

        var expected = NormalizeText(expectedText);
        var actual = NormalizeText(actualText);

        return actual.Contains(expected) || expected.Contains(actual) || actual.Equals(expected);
    }

    private static string NormalizeText(string input)
    {
        return (input ?? string.Empty)
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .ToLowerInvariant()
            .Trim();
    }

    private string CaptureScreenshot(string testCaseId)
    {
        try
        {
            string screenshotDirectory = Path.Combine(AppContext.BaseDirectory, "Screenshots");
            if (!Directory.Exists(screenshotDirectory))
            {
                Directory.CreateDirectory(screenshotDirectory);
            }

            string fileName = $"{testCaseId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string screenshotPath = Path.Combine(screenshotDirectory, fileName);

            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(screenshotPath);

            return screenshotPath;
        }
        catch (Exception ex)
        {
            return $"Failed to capture screenshot: {ex.Message}";
        }
    }

    private void LogTestCaseInfo(string testCaseId, string function, string bigItem)
    {
        TestContext.Out.WriteLine("═══════════════════════════════════════");
        TestContext.Out.WriteLine($"Test Case ID: {testCaseId}");
        TestContext.Out.WriteLine($"Function: {function}");
        TestContext.Out.WriteLine($"Big Item: {bigItem}");
        TestContext.Out.WriteLine("═══════════════════════════════════════");
        TestContext.Out.WriteLine();
    }

    private void LogTestResults(string expected, string actual, string status, string updatedRange)
    {
        TestContext.Out.WriteLine();
        TestContext.Out.WriteLine("───────────────────────────────────────");
        TestContext.Out.WriteLine($"Expected Result: {expected}");
        TestContext.Out.WriteLine($"Actual Result: {actual}");
        TestContext.Out.WriteLine($"Test Status: {status}");
        TestContext.Out.WriteLine($"Excel Updated: {updatedRange}");
        TestContext.Out.WriteLine("───────────────────────────────────────");
    }
}