using System.Globalization;
using System.Text;
using DoAnCuoiKy.Models;
using DoAnCuoiKy_TH.DataProviders;
using DoAnCuoiKy_TH.Pages;
using DoAnCuoiKy_TH.Utilities;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DoAnCuoiKy_TH.Tests.Banking;

[TestFixture]
[Explicit("Requires Parabank web account and browser automation setup")]
public class BankingFeaturesTests : DriverFactory
{
    private const string ExcelSheetName = "TC_khoa";
    private const string TestCasePrefixFilter = "TC_";
    private static readonly string[] SelectedTestCaseIds =
    {
        "TC_FT_01",
        "TC_FT_02",
        "TC_FT_03",
        "TC_FT_04",
        "TC_ONA_01",
        "TC_ONA_02",
        "TC_ONA_03",
        "TC_ONA_04",
        "TC_SM_01",
        "TC_SM_02"
    };

    private const string UsernameEnvironmentVariable = "PARABANK_USERNAME";
    private const string PasswordEnvironmentVariable = "PARABANK_PASSWORD";
    private const string DefaultUsername = "khoamedia123";
    private const string DefaultPassword = "khoamedia123";
    private const string LoginPageUrl = "https://parabank.parasoft.com/parabank/index.htm";

    [TestCaseSource(nameof(GetSelectedBankingTestCases))]
    public void ExecuteSelectedBankingCase(string testCaseId)
    {
        ExecuteBankingFeatureTestCaseById(testCaseId);
    }

    private static IEnumerable<string> GetSelectedBankingTestCases()
    {
        return SelectedTestCaseIds;
    }

    private void ExecuteBankingFeatureTestCaseById(string testCaseId)
    {
        var testCase = ExcelDataProvider.GetTestCaseById(ExcelSheetName, TestCasePrefixFilter, testCaseId);
        var function = testCase.Function;
        var bigItem = testCase.BigItem;
        var scenario = ResolveScenario(testCaseId, function);

        LogTestCaseInfo(testCaseId, function, bigItem);

        string actualResult;
        bool isTestPassed;
        string notes = string.Empty;

        try
        {
            var fallbackData = testCase.Steps
                .Select(step => ConvertExcelValue(step.TestData))
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

            if (scenario != "smoke")
            {
                LoginToParabank();
            }

            OpenScenarioPage(scenario);

            foreach (var step in testCase.Steps)
            {
                TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                var stepData = string.IsNullOrWhiteSpace(step.TestData) ? fallbackData : step.TestData ?? string.Empty;
                TestContext.Out.WriteLine($"Resolved data -> '{stepData}'");
                ExecuteTestStep(testCaseId, testCase.Function, step, stepData);
            }

            actualResult = GetActualResult(testCaseId, testCase.Function);
            isTestPassed = IsResultMatchingExpected(testCase.Expected, actualResult);
        }
        catch (Exception ex)
        {
            actualResult = ex.Message;
            isTestPassed = false;
        }

        if (!isTestPassed)
        {
            var screenshotPath = CaptureScreenshot(testCaseId);
            notes = $"Screenshot: {screenshotPath}";
            TestContext.Out.WriteLine($"Screenshot saved at: {screenshotPath}");
        }

        var resultStatus = isTestPassed ? "PASS" : "FAIL";
        var updatedExcelRange = ExcelDataProvider.WriteTestResults(testCase.Steps, actualResult, resultStatus, ExcelSheetName, notes);

        LogTestResults(testCase.Expected, actualResult, resultStatus, updatedExcelRange);

        Assert.That(isTestPassed, Is.True, $"Test case {testCaseId} failed. Expected: {testCase.Expected}, Actual: {actualResult}");
    }

    private void OpenScenarioPage(string scenario)
    {
        if (scenario == "find-transactions")
        {
            new FindTransactionsPage(driver).Open();
            return;
        }

        if (scenario == "open-new-account")
        {
            new OpenNewAccountPage(driver).Open();
        }
    }

    private void LoginToParabank()
    {
        var username = Environment.GetEnvironmentVariable(UsernameEnvironmentVariable) ?? DefaultUsername;
        var password = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable) ?? DefaultPassword;

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            driver.Navigate().GoToUrl(LoginPageUrl);

            var loginPage = new LoginPage(driver);
            loginPage.EnterUsername(username);
            loginPage.EnterPassword(password);
            loginPage.ClickLogin();

            if (TryWaitForLoginResult(out var loginError) && driver.Url.Contains("overview", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (attempt == 2)
            {
                throw new InvalidOperationException($"Đăng nhập thất bại: {loginError}");
            }
        }
    }

    private void ExecuteTestStep(string testCaseId, string function, TestStep step, string stepData)
    {
        var scenario = ResolveScenario(testCaseId, function);

        if (scenario == "find-transactions")
        {
            ExecuteFindTransactionsStep(step, stepData);
            return;
        }

        if (scenario == "open-new-account")
        {
            ExecuteOpenNewAccountStep(step, stepData);
            return;
        }

        if (scenario == "smoke")
        {
            ExecuteSmokeStep(step, stepData);
            return;
        }

        throw new NotSupportedException($"Chưa hỗ trợ function/scenario: {function} ({testCaseId})");
    }

    private void ExecuteFindTransactionsStep(TestStep step, string stepData)
    {
        var page = new FindTransactionsPage(driver);
        var action = NormalizeText(step.StepAction ?? string.Empty);
        var data = ConvertExcelValue(stepData);

        if (action.Contains("mo trang") || action.Contains("open") || action.Contains("find transactions"))
        {
            page.Open();
            return;
        }

        if ((action.Contains("id") || action.Contains("transaction id")) && action.Contains("nhap"))
        {
            page.EnterTransactionId(data);
            return;
        }

        if ((action.Contains("date range") || action.Contains("khoang ngay") || action.Contains("from") || action.Contains("to")) && action.Contains("nhap"))
        {
            var dates = ParseDateRange(data);
            page.EnterTransactionDateRange(dates.fromDate, dates.toDate);
            return;
        }

        if ((action.Contains("transaction date") || action.Contains("ngay")) && action.Contains("nhap"))
        {
            var normalizedDate = ConvertExcelDate(data);
            page.EnterTransactionDateRange(normalizedDate, normalizedDate);
            return;
        }

        if ((action.Contains("amount") || action.Contains("so tien")) && action.Contains("nhap"))
        {
            page.EnterAmount(data);
            return;
        }

        if (action.Contains("find") || action.Contains("nhan"))
        {
            page.Submit();
        }
    }

    private void ExecuteSmokeStep(TestStep step, string stepData)
    {
        var action = NormalizeText(step.StepAction ?? string.Empty);
        var data = ConvertExcelValue(stepData);

        if (string.IsNullOrWhiteSpace(action))
        {
            return;
        }

        if (action.Contains("find transaction") || action.Contains("tim giao dich"))
        {
            new FindTransactionsPage(driver).Open();
            return;
        }

        if (action.Contains("open new account") || action.Contains("mo tai khoan"))
        {
            new OpenNewAccountPage(driver).Open();
            return;
        }

        if ((action.Contains("nhap user") || action.Contains("user/pass") || action.Contains("username")) && !string.IsNullOrWhiteSpace(data))
        {
            var (username, password) = ParseCredentials(data);
            var loginPage = new LoginPage(driver);
            loginPage.EnterUsername(username);
            loginPage.EnterPassword(password);
            return;
        }

        if (action.Contains("login") || action.Contains("dang nhap"))
        {
            var loginPage = new LoginPage(driver);
            loginPage.ClickLogin();
        }
    }

    private void ExecuteOpenNewAccountStep(TestStep step, string stepData)
    {
        var page = new OpenNewAccountPage(driver);
        var action = NormalizeText(step.StepAction ?? string.Empty);
        var data = ConvertExcelValue(stepData);

        if (action.Contains("open new account") || action.Contains("vao menu"))
        {
            page.Open();
            return;
        }

        if (action.Contains("chon") && (action.Contains("loai") || action.Contains("checking") || action.Contains("saving")))
        {
            var accountType = data;
            if (string.IsNullOrWhiteSpace(accountType))
            {
                accountType = action.Contains("saving") ? "SAVINGS" : "CHECKING";
            }

            page.SelectAccountType(accountType);
            return;
        }

        if (action.Contains("chon tai khoan nguon") || action.Contains("from account"))
        {
            page.SelectFromAccount(data);
            return;
        }

        if (action.Contains("click") || action.Contains("nhan") || action.Contains("open"))
        {
            page.OpenAccount();
        }
    }

    private string GetActualResult(string testCaseId, string function)
    {
        var scenario = ResolveScenario(testCaseId, function);

        if (scenario == "find-transactions")
        {
            var page = new FindTransactionsPage(driver);

            if (page.HasValidationError())
            {
                return "Báo lỗi";
            }

            return page.HasTransactions()
                ? "Hiển thị đúng giao dịch"
                : "Không có kết quả";
        }

        if (scenario == "open-new-account")
        {
            var page = new OpenNewAccountPage(driver);
            return page.IsAccountCreated()
                ? "Tạo thành công"
                : "Không tạo được tài khoản mới";
        }

        if (scenario == "smoke")
        {
            if (TryWaitForLoginResult(out var loginError) && driver.Url.Contains("overview", StringComparison.OrdinalIgnoreCase))
            {
                return "Đăng nhập OK";
            }

            return string.IsNullOrWhiteSpace(loginError) ? "Smoke fail" : "Báo lỗi";
        }

        return "Không xác định được function";
    }

    private static string ResolveScenario(string testCaseId, string function)
    {
        var normalizedId = NormalizeText(testCaseId);
        if (normalizedId.StartsWith("tc_ft_", StringComparison.Ordinal))
        {
            return "find-transactions";
        }

        if (normalizedId.StartsWith("tc_ona_", StringComparison.Ordinal))
        {
            return "open-new-account";
        }

        if (normalizedId.StartsWith("tc_sm_", StringComparison.Ordinal))
        {
            return "smoke";
        }

        var normalizedFunction = NormalizeText(function);

        if (normalizedFunction.Contains("find transactions") || normalizedFunction.Contains("tim giao dich"))
        {
            return "find-transactions";
        }

        if (normalizedFunction.Contains("open new account") || normalizedFunction.Contains("mo tai khoan"))
        {
            return "open-new-account";
        }

        if (normalizedFunction.Contains("smoke"))
        {
            return "smoke";
        }

        return "unknown";
    }

    private static bool IsResultMatchingExpected(string expectedText, string actualText)
    {
        if (string.IsNullOrWhiteSpace(expectedText) || string.IsNullOrWhiteSpace(actualText))
        {
            return false;
        }

        var expected = NormalizeText(expectedText);
        var actual = NormalizeText(actualText);

        if (actual.Contains(expected) || expected.Contains(actual))
        {
            return true;
        }

        if (expected.Contains("hien thi giao dich") && actual.Contains("hien thi giao dich"))
        {
            return true;
        }

        if (expected.Contains("hien thi dung giao dich") && actual.Contains("hien thi dung giao dich"))
        {
            return true;
        }

        if (expected.Contains("khong co ket qua") && actual.Contains("khong co ket qua"))
        {
            return true;
        }

        if (expected.Contains("khong hien thi giao dich") && actual.Contains("khong hien thi giao dich"))
        {
            return true;
        }

        if (expected.Contains("bao loi") && (actual.Contains("bao loi") || actual.Contains("loi")))
        {
            return true;
        }

        if (expected.Contains("dang nhap ok") && actual.Contains("dang nhap ok"))
        {
            return true;
        }

        if (expected.Contains("thanh cong") && (actual.Contains("thanh cong") || actual.Contains("duoc tao")))
        {
            return true;
        }

        if ((expected.Contains("tao account") || expected.Contains("tao tai khoan"))
            && (actual.Contains("tao thanh cong") || actual.Contains("duoc tao") || actual.Contains("thanh cong")))
        {
            return true;
        }

        if (expected.Contains("loi gia tri khong hop le") && actual.Contains("loi gia tri khong hop le"))
        {
            return true;
        }

        if (expected.Contains("khoan vay duoc duyet") && actual.Contains("khoan vay duoc duyet"))
        {
            return true;
        }

        if (expected.Contains("khoan vay bi tu choi") && actual.Contains("khoan vay bi tu choi"))
        {
            return true;
        }

        if (expected.Contains("nhap day du thong tin") && actual.Contains("nhap day du thong tin"))
        {
            return true;
        }

        return expected.Contains("tai khoan") && expected.Contains("duoc tao") && actual.Contains("tai khoan") && actual.Contains("duoc tao");
    }

    private static string NormalizeText(string input)
    {
        var normalized = RemoveDiacritics(input ?? string.Empty)
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ")
            .ToLowerInvariant()
            .Trim();

        while (normalized.Contains("  ", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);
        }

        return normalized;
    }

    private static string RemoveDiacritics(string input)
    {
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var character in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(character);
            }
        }

        return stringBuilder
            .ToString()
            .Replace('đ', 'd')
            .Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormC);
    }

    private static string ConvertExcelValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim();
    }

    private static string ConvertExcelDate(string value)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
            || DateTime.TryParse(value, new CultureInfo("vi-VN"), DateTimeStyles.None, out parsedDate))
        {
            return parsedDate.ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var serialDate))
        {
            return DateTime.FromOADate(serialDate).ToString("MM-dd-yyyy", CultureInfo.InvariantCulture);
        }

        return value;
    }

    private static (string fromDate, string toDate) ParseDateRange(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (string.Empty, string.Empty);
        }

        var parts = input.Split(" - ", StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return (input, input);
        }

        return (ConvertExcelDate(parts[0]), ConvertExcelDate(parts[1]));
    }

    private static (string username, string password) ParseCredentials(string data)
    {
        var separators = new[] { '/', '|', ';', ',' };
        var parts = data.Split(separators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return (parts[0], parts[1]);
        }

        return (DefaultUsername, DefaultPassword);
    }

    private bool TryWaitForLoginResult(out string loginError)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        try
        {
            wait.Until(_ =>
                driver.Url.Contains("overview", StringComparison.OrdinalIgnoreCase)
                || !string.IsNullOrWhiteSpace(new LoginPage(driver).GetErrorMessage()));
        }
        catch (WebDriverTimeoutException)
        {
            // Fall through and inspect current page state.
        }

        loginError = new LoginPage(driver).GetErrorMessage();
        return driver.Url.Contains("overview", StringComparison.OrdinalIgnoreCase)
            || !string.IsNullOrWhiteSpace(loginError);
    }

    private string CaptureScreenshot(string testCaseId)
    {
        try
        {
            var screenshotDirectory = Path.Combine(AppContext.BaseDirectory, "Screenshots");
            if (!Directory.Exists(screenshotDirectory))
            {
                Directory.CreateDirectory(screenshotDirectory);
            }

            var fileName = $"{testCaseId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var screenshotPath = Path.Combine(screenshotDirectory, fileName);

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
