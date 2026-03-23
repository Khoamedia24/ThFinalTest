using DoAnCuoiKy_TH.DataProviders;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ParaBankAutomation.Pages;
using ParaBankAutomation.Utilities;
using System.Globalization;
using THFinalTest.ParaBankAutomation.Models;
using THFinalTest.ParaBankAutomation.Pages;

namespace ParaBankAutomation.Tests
{
    [TestFixture]
    public class BillPayTests : DriverFactory
    {
        #region Constants

        private const string ExcelSheetName = "TestCase_Tam";
        private const string TestCasePrefixFilter = "TC_";

        // NHỚ CẬP NHẬT 2 SỐ NÀY CHO KHỚP VỚI MODULE BILL PAY TRONG FILE EXCEL
        private const int MinEnabledTestCaseNumber = 22;

        private const int MaxEnabledTestCaseNumber = 30;

        #endregion Constants

        private BillPayPage? billPayPage;
        private LoginPage? loginPage;

        [TestCaseSource(nameof(GetVisibleBillPayTestCases))]
        public void ExecuteBillPayTestCase(string testCaseId, string function, string bigItem)
        {
            if (!ShouldRunTestCase(testCaseId, out _))
            {
                Assert.Ignore($"Skipping {testCaseId}. Only {TestCasePrefixFilter}{MinEnabledTestCaseNumber:00} to {TestCasePrefixFilter}{MaxEnabledTestCaseNumber:00} are enabled.");
            }

            var testCase = ExcelDataProvider.GetTestCaseById(ExcelSheetName, TestCasePrefixFilter, testCaseId);
            billPayPage = new BillPayPage(driver);
            loginPage = new LoginPage(driver);

            LogTestCaseInfo(testCaseId, function, bigItem);

            string actualResult;
            bool isTestPassed;
            string notes = "";

            try
            {
                // ========================================== BƯỚC 1: XỬ LÝ TIỀN ĐIỀU KIỆN
                // (PRE-CONDITION) ==========================================
                TestContext.Out.WriteLine("Thực hiện Tiền điều kiện: Đăng nhập hệ thống...");
                driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");

                // Sử dụng tài khoản hợp lệ để qua cổng đăng nhập
                loginPage.EnterUsername("mukunthan");
                loginPage.EnterPassword("mukunthan");
                loginPage.ClickLogin();

                if (!driver.Url.Contains("overview"))
                {
                    throw new Exception("Tiền điều kiện thất bại: Đăng nhập không thành công, không thể test Bill Pay.");
                }

                TestContext.Out.WriteLine("Tiền điều kiện hoàn tất. Chuyển sang trang Bill Pay...");
                driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/billpay.htm");

                // ========================================== BƯỚC 2: THỰC THI CÁC BƯỚC TEST TỪ
                // EXCEL ==========================================
                foreach (var step in testCase.Steps)
                {
                    TestContext.Out.WriteLine($"Step {step.StepNumber} - {step.StepAction} - {step.TestData}");
                    ExecuteTestStep(step);
                }

                // ========================================== BƯỚC 3: CHỜ ĐỘNG (EXPLICIT WAIT) XỬ LÝ
                // TIMING ========================================== Cho bot đợi tối đa 5 giây cho
                // đến khi trang thực sự trả về thông báo
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                try
                {
                    wait.Until(d =>
                        !string.IsNullOrEmpty(billPayPage.GetErrorMessage()) ||
                        !string.IsNullOrEmpty(billPayPage.GetSuccessMessage())
                    );
                }
                catch (WebDriverTimeoutException)
                {
                    TestContext.Out.WriteLine("[DEBUG] Hết 5s chờ mà chưa thấy thông báo hiện ra. Vẫn tiếp tục lấy kết quả...");
                }

                // ========================================== BƯỚC 4: LẤY KẾT QUẢ VÀ SO SÁNH ==========================================
                actualResult = GetActualResult();
                isTestPassed = IsResultMatchingExpected(testCase.Expected, actualResult);
            }
            catch (Exception ex)
            {
                actualResult = ex.Message;
                isTestPassed = false;
            }

            // Xử lý chụp ảnh và ghi Excel nếu Fail
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

        // --- BỘ THÔNG DỊCH STEP ACTION ---
        private void ExecuteTestStep(TestStep step)
        {
            var action = step.StepAction?.ToLower() ?? "";
            var testData = step.TestData ?? "";

            if (billPayPage == null) throw new InvalidOperationException("BillPayPage is not initialized");

            if (action.Contains("mở trang") || action.Contains("open")) driver.Navigate().GoToUrl(testData);
            else if (action.Contains("payee name") || action.Contains("tên người nhận")) billPayPage.EnterPayeeName(testData);
            else if (action.Contains("address") || action.Contains("địa chỉ")) billPayPage.EnterAddress(testData);
            else if (action.Contains("city") || action.Contains("thành phố")) billPayPage.EnterCity(testData);
            else if (action.Contains("state") || action.Contains("tiểu bang")) billPayPage.EnterState(testData);
            else if (action.Contains("zip code") || action.Contains("mã bưu điện")) billPayPage.EnterZipCode(testData);
            else if (action.Contains("phone") || action.Contains("điện thoại")) billPayPage.EnterPhone(testData);
            else if (action.Contains("verify account") || action.Contains("xác nhận tài khoản")) billPayPage.EnterVerifyAccount(testData);
            else if (action.Contains("account") || action.Contains("tài khoản")) billPayPage.EnterAccount(testData);
            else if (action.Contains("amount") || action.Contains("số tiền")) billPayPage.EnterAmount(testData);
            else if (action.Contains("send payment") || action.Contains("thanh toán") || action.Contains("click")) billPayPage.ClickSendPayment();
        }

        // --- HÀM LẤY KẾT QUẢ THỰC TẾ ---
        private string GetActualResult()
        {
            if (billPayPage == null) throw new InvalidOperationException("BillPayPage is not initialized");

            try
            {
                // Kiểm tra các dòng text lỗi đỏ trên màn hình
                string errorMessage = billPayPage.GetErrorMessage();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return errorMessage;
                }

                // Kiểm tra tiêu đề thành công
                string successMessage = billPayPage.GetSuccessMessage();
                if (successMessage.Contains("Complete") || successMessage.Contains("Successful"))
                {
                    return "Thanh toán thành công";
                }

                return "Không xác định được trạng thái";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống: {ex.Message}";
            }
        }

        // --- CÁC HÀM PHỤ TRỢ ---
        private static IEnumerable<object[]> GetVisibleBillPayTestCases()
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
        { }

        private void LogTestResults(string expected, string actual, string status, string updatedRange)
        { }
    }
}